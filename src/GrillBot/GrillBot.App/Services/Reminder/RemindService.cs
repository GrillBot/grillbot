using GrillBot.App.Infrastructure;
using GrillBot.Database.Entity;

namespace GrillBot.App.Services.Reminder;

[Initializable]
public class RemindService
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public RemindService(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
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
}
