using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Extensions;
using GrillBot.Data.Models.API;

namespace GrillBot.App.Actions.Api.V2;

public class GetTodayBirthdayInfo : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }
    private IConfiguration Configuration { get; }
    private ITextsManager Texts { get; }

    public GetTodayBirthdayInfo(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder,
        IDiscordClient discordClient, IConfiguration configuration, ITextsManager texts) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        DiscordClient = discordClient;
        Configuration = configuration;
        Texts = texts;
    }

    public async Task<MessageResponse> ProcessAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var todayBirthdayUsers = await repository.User.GetUsersWithTodayBirthday();
        var users = await TransformUsersAsync(todayBirthdayUsers);
        var message = Format(users);
        return new MessageResponse(message);
    }

    private async Task<List<(IUser user, int? age)>> TransformUsersAsync(List<Database.Entity.User> users)
    {
        var result = new List<(IUser user, int? age)>();

        foreach (var user in users)
        {
            var discordUser = await DiscordClient.FindUserAsync(user.Id.ToUlong());
            if (discordUser == null) continue;

            var age = user.BirthdayAcceptYear ? ComputeAge(user.Birthday!.Value) : (int?)null;
            result.Add((discordUser, age));
        }

        return result;
    }

    private string Format(IReadOnlyCollection<(IUser user, int? age)> users)
    {
        if (users.Count == 0)
            return Texts["BirthdayModule/Info/NoOneHave", ApiContext.Language].FormatWith(Configuration["Discord:Emotes:Sadge"]);

        var formatted = users
            .Select(o =>
                o.age == null
                    ? Texts["BirthdayModule/Info/Parts/WithoutYears", ApiContext.Language].FormatWith(o.user.GetDisplayName(false))
                    : Texts["BirthdayModule/Info/Parts/WithYears", ApiContext.Language].FormatWith(o.user.GetDisplayName(false), o.age.Value)
            ).ToList();

        var result = Texts[$"BirthdayModule/Info/Template/{(users.Count > 1 ? "MultipleForm" : "SingleForm")}", ApiContext.Language];
        var hypers = Configuration["Discord:Emotes:Hypers"];

        if (users.Count > 1)
        {
            var withoutLast = string.Join(", ", formatted.Take(formatted.Count - 1));
            result = result.FormatWith(withoutLast, formatted[^1], hypers);
        }
        else
        {
            result = result.FormatWith(formatted[0], hypers);
        }

        return result;
    }

    private static int ComputeAge(DateTime dateTime)
    {
        var today = DateTime.Today;
        var age = today.Year - dateTime.Year;
        if (dateTime.Date > today.AddYears(-age)) age--;

        return age;
    }
}
