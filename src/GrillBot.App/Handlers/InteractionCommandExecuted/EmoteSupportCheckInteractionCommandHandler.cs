using Discord.Interactions;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Extensions;
using GrillBot.Core.Managers.Discord;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Handlers.InteractionCommandExecuted;

public class EmoteSupportCheckInteractionCommandHandler : IInteractionCommandExecutedEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IEmoteManager EmoteManager { get; }

    public EmoteSupportCheckInteractionCommandHandler(GrillBotDatabaseBuilder databaseBuilder, IEmoteManager emoteManager)
    {
        DatabaseBuilder = databaseBuilder;
        EmoteManager = emoteManager;
    }

    public async Task ProcessAsync(ICommandInfo commandInfo, IInteractionContext context, IResult result)
    {
        if (context.Interaction is not ISlashCommandInteraction interaction || interaction.IsDMInteraction)
            return;

        var emotes = interaction.Data.Options
            .Flatten(o => o.Options)
            .Where(o => o.Type is ApplicationCommandOptionType.String)
            .Select(o => Emote.TryParse(o.Value.ToString(), out var emote) ? emote : null)
            .Where(o => o is not null)
            .ToList();

        await using var repository = DatabaseBuilder.CreateRepository();
        var supportedEmotes = (await EmoteManager.GetSupportedEmotesAsync()).ConvertAll(o => o.ToString());

        foreach (var emote in emotes)
            await ProcessEmoteAsync(repository, emote!, supportedEmotes);
        await repository.CommitAsync();
    }

    private static async Task ProcessEmoteAsync(GrillBotRepository repository, IEmote emote, List<string> supportedEmotes)
    {
        var emoteId = emote.ToString()!;
        if (!supportedEmotes.Contains(emoteId))
            return;

        var dbEmotes = await repository.Emote.FindStatisticsByEmoteIdAsync(emoteId);
        foreach (var dbEmote in dbEmotes)
            dbEmote.IsEmoteSupported = true;
    }
}
