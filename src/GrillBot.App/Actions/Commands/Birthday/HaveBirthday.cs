namespace GrillBot.App.Actions.Commands.Birthday;

public class HaveBirthday : CommandAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public HaveBirthday(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task<bool> ProcessAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var dbUser = await repository.User.FindUserAsync(Context.User);
        return dbUser?.Birthday != null;
    }
}
