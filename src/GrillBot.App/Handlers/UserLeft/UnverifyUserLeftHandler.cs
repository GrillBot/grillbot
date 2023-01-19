using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.UserLeft;

public class UnverifyUserLeftHandler : IUserLeftEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public UnverifyUserLeftHandler(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(IGuild guild, IUser user)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var unverify = await repository.Unverify.FindUnverifyAsync(guild.Id, user.Id);
        if (unverify == null) return;

        repository.Remove(unverify);
        await repository.CommitAsync();
    }
}
