using Discord.Interactions;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Common.Managers.Localization;

namespace GrillBot.App.Handlers.InteractionCommandExecuted;

public class UpdateUserLanguageHandler : IInteractionCommandExecutedEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public UpdateUserLanguageHandler(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(ICommandInfo commandInfo, IInteractionContext context, IResult result)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var user = await repository.User.FindUserAsync(context.User);
        if (user == null) return;

        user.Language = TextsManager.FixLocale(context.Interaction.UserLocale);
        await repository.CommitAsync();
    }
}
