using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;

namespace GrillBot.App.Actions.Api.V2;

public class GetTodayBirthdayInfo : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }
    private IConfiguration Configuration { get; }

    public GetTodayBirthdayInfo(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder,
        IDiscordClient discordClient, IConfiguration configuration) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        DiscordClient = discordClient;
        Configuration = configuration;
    }

    public async Task<string> ProcessAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var todayBirthdayUsers = await repository.User.GetUsersWithTodayBirthday();
        var users = await TransformUsersAsync(todayBirthdayUsers);
        return Format(users);
    }

    private async Task<List<(IUser user, int? age)>> TransformUsersAsync(List<Database.Entity.User> users)
    {
        var result = new List<(IUser user, int? age)>();

        foreach (var user in users)
        {
            var discordUser = await DiscordClient.FindUserAsync(user.Id.ToUlong());
            if (discordUser == null) continue;

            var age = user.BirthdayAcceptYear ? user.Birthday!.Value.ComputeAge() : (int?)null;
            result.Add((discordUser, age));
        }

        return result;
    }

    private string Format(List<(IUser user, int? age)> users)
    {
        if (users.Count == 0)
        {
            return $"Dnes nemá nikdo narozeniny {Configuration["Discord:Emotes:Sadge"]}";
        }

        var withoutLast = users.Take(users.Count - 1).Select(o => $"**{o.user.GetDisplayName(false)}{(o.age != null ? $" ({o.age} let)" : null)}**".Trim());
        var last = users[^1];

        var builder = new StringBuilder("Dnes ")
            .Append(users.Count == 1 ? "má" : "mají").Append(" narozeniny ")
            .AppendJoin(", ", withoutLast);

        if (users.Count > 1)
            builder.Append(" a ");

        builder.Append("**").Append(last.user.GetDisplayName(false));
        if (last.age != null)
            builder.Append(" (").Append(last.age).Append(" let)**");
        else
            builder.Append("**");

        builder.Append(' ').Append(Configuration["Discord:Emotes:Hypers"]);
        return builder.ToString();
    }
}
