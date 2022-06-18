using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;

namespace GrillBot.App.Services.Birthday;

public class BirthdayService
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }

    public BirthdayService(IDiscordClient client, GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
        DiscordClient = client;
    }

    public async Task AddBirthdayAsync(IUser user, DateTime birthday)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var dbUser = await repository.User.GetOrCreateUserAsync(user);
        dbUser.Birthday = birthday.Date;

        await repository.CommitAsync();
    }

    public async Task RemoveBirthdayAsync(IUser user)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var dbUser = await repository.User.FindUserAsync(user);
        if (dbUser == null) return;

        dbUser.Birthday = null;
        await repository.CommitAsync();
    }

    public async Task<bool> HaveBirthdayAsync(IUser user)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var dbUser = await repository.User.FindUserAsync(user);
        return dbUser?.Birthday != null;
    }

    public async Task<List<(IUser user, int? age)>> GetTodayBirthdaysAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var users = await repository.User.GetUsersWithTodayBirthday();
        var result = new List<(IUser user, int? age)>();

        foreach (var entity in users)
        {
            var user = await DiscordClient.FindUserAsync(entity.Id.ToUlong());

            if (user != null)
                result.Add((user, entity.BirthdayAcceptYear ? entity.Birthday!.Value.ComputeAge() : null));
        }

        return result;
    }
}
