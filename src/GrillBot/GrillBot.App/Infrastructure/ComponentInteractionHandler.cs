using GrillBot.App.Infrastructure.Embeds;

namespace GrillBot.App.Infrastructure;

public abstract class ComponentInteractionHandler
{
    public abstract Task ProcessAsync(IInteractionContext context);

    protected static bool TryParseData<TMetadata>(IDiscordInteraction interaction, out SocketMessageComponent component, out TMetadata metadata) where TMetadata : IEmbedMetadata, new()
    {
        metadata = default;
        component = interaction as SocketMessageComponent;

        var embed = component?.Message.Embeds.FirstOrDefault();
        if (embed?.Footer == null || embed.Author == null)
            return false;

        return embed.TryParseMetadata(out metadata);
    }

    protected static bool TryParseMesasge(IDiscordInteraction interaction, out IUserMessage message)
    {
        message = null;
        if (interaction is not IComponentInteraction componentInteraction) return false;

        message = componentInteraction.Message;
        return true;
    }

    protected static int CheckNewPageNumber(int newPage, int maxPages)
    {
        if (newPage >= maxPages) return maxPages - 1;
        return newPage < 0 ? 0 : newPage;
    }
}
