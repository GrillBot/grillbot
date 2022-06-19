using Discord.Commands;
using System.Text.RegularExpressions;
using GrillBot.Common.Extensions.Discord;

namespace GrillBot.App.Infrastructure.TypeReaders.Implementations;

public class UserConverter : ConverterBase<IUser>
{
    public UserConverter(IServiceProvider provider, ICommandContext context) : base(provider, context)
    {
    }

    public UserConverter(IServiceProvider provider, IInteractionContext context) : base(provider, context)
    {
    }

    public override async Task<IUser> ConvertAsync(string value)
    {
        // Match on caller
        value = value.Trim();
        if (Regex.IsMatch(value, "^(me|j[a|á])$", RegexOptions.IgnoreCase)) return User;

        if (Guild == null)
        {
            var discriminatorIndex = value.LastIndexOf("#", StringComparison.Ordinal);

            IUser user;
            if (discriminatorIndex >= 0)
            {
                var username = value[..discriminatorIndex];

                if (ushort.TryParse(value[(discriminatorIndex + 1)..], out _))
                    user = await Client.FindUserAsync(username, value[(discriminatorIndex + 1)..]);
                else
                    user = await Client.FindUserAsync(username, null);
            }
            else
            {
                user = await Client.FindUserAsync(value, null);
            }

            if (user != null)
                return user;
        }
        else
        {
            var users = await Guild.GetUsersAsync();
            var matches = users
                .Where(o => (!string.IsNullOrEmpty(o.Nickname) && o.Nickname.Contains(value, StringComparison.CurrentCultureIgnoreCase)) ||
                            o.Username.Contains(value, StringComparison.CurrentCultureIgnoreCase))
                .ToList();

            if (matches.Count == 1) return matches[0];

            // Finds user directly from discord and get guild user from memory.
            matches = (await Guild.SearchUsersAsync(value)).Where(o => o != null).ToList();
            if (matches.Count == 1)
                return await Guild.GetUserAsync(matches[0].Id) ?? matches[0];
        }

        return null;
    }
}
