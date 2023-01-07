namespace GrillBot.App.Actions.Commands.Birthday;

public class RemoveBirthday : CommandAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public RemoveBirthday(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var dbUser = await repository.User.FindUserAsync(Context.User);
        if (dbUser == null) return;

        dbUser.Birthday = null;
        await repository.CommitAsync();
    }
}
