using GrillBot.App.Infrastructure;
using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Localization;
using GrillBot.Database.Entity;

namespace GrillBot.App.Services.Reminder;

[Initializable]
public class RemindService
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }

    public RemindService(GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
    }

    public async Task<int> GetRemindersCountAsync(IUser forUser)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        return await repository.Remind.GetRemindersCountAsync(forUser);
    }

    public async Task<List<RemindMessage>> GetRemindersAsync(IUser forUser, int page)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        return await repository.Remind.GetRemindersPageAsync(forUser, page);
    }

    public async Task<Dictionary<long, string>> GetRemindSuggestionsAsync(IUser user, string locale)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.Remind.GetRemindSuggestionsAsync(user);
        var userId = user.Id.ToString();

        var incoming = data
            .Where(o => o.ToUserId == userId)
            .ToDictionary(o => o.Id, o => Texts["RemindModule/Suggestions/Incoming", locale].FormatWith(o.Id, o.At.ToCzechFormat(), o.FromUser!.FullName()));

        var outgoing = data
            .Where(o => o.FromUserId == userId)
            .ToDictionary(o => o.Id, o => Texts["RemindModule/Suggestions/Outgoing", locale].FormatWith(o.Id, o.At.ToCzechFormat(), o.ToUser!.FullName()));

        return incoming
            .Concat(outgoing)
            .DistinctBy(o => o.Key)
            .OrderBy(o => o.Key)
            .Take(25)
            .ToDictionary(o => o.Key, o => o.Value);
    }
}
