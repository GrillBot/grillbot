namespace GrillBot.App.Services.Birthday;

public class BirthdayService
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public BirthdayService(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
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
}
