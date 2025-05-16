using Discord.Interactions;
using GrillBot.Common.Helpers;
using GrillBot.Core.Extensions;

namespace GrillBot.App.Infrastructure.TypeReaders;

public class UsersTypeConverter : TypeConverter<IEnumerable<IUser>>
{
    public override ApplicationCommandOptionType GetDiscordType()
        => ApplicationCommandOptionType.String;

    public override async Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
    {
        var value = (string)option.Value;
        var result = new List<IUser>();
        var locale = context.Interaction.UserLocale;

        foreach (var userIdent in value.Split(' ').Where(o => !string.IsNullOrEmpty(o)).Select(o => o.Trim()))
        {
            var user = await ConvertUserAsync(context, userIdent);
            if (user is null)
                return TypeReaderHelper.ConvertFailed(services, "UserNotFound", locale);
            result.Add(user);
        }

        return TypeReaderHelper.FromSuccess(result.ToArray());
    }

    private static async Task<IUser?> ConvertUserAsync(IInteractionContext context, string userIdent)
    {
        // Match caller.
        if (UserRegexHelper.IsMeRegex().IsMatch(userIdent))
            return context.User;

        // Match mentions
        var userIdMatch = UserRegexHelper.UserIdRegex().Match(userIdent);
        if (userIdMatch.Success)
        {
            var user = await context.Guild.GetUserAsync(userIdMatch.Groups[1].Value.ToUlong());
            if (user is not null)
                return user;
        }

        var users = await context.Guild.GetUsersAsync();
        var matches = users.Where(o =>
            (!string.IsNullOrEmpty(o.Nickname) && o.Nickname.Contains(userIdent, StringComparison.CurrentCultureIgnoreCase)) ||
            o.Username.Contains(userIdent, StringComparison.CurrentCultureIgnoreCase)
        ).ToList();

        if (matches.Count == 1)
            return matches[0];
        if (userIdent.Length > 100)
            return null;

        // Finds user directly from discord and get guild user from memory.
        matches = (await context.Guild.SearchUsersAsync(userIdent)).Where(o => o != null).ToList();
        if (matches.Count == 1)
            return await context.Guild.GetUserAsync(matches[0].Id) ?? matches[0];

        return null;
    }
}
