namespace GrillBot.App.Actions.Commands.Birthday;

public class AddBirthday : CommandAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public AddBirthday(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(DateTime birthday)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var dbUser = await repository.User.GetOrCreateUserAsync(Context.User);
        dbUser.Birthday = birthday.Date;

        await repository.CommitAsync();
    }
}
