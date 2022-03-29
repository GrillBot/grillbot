using GrillBot.App.Infrastructure.Embeds;

namespace GrillBot.App.Infrastructure;

public abstract class ComponentInteractionHandler
{
    public abstract Task ProcessAsync(IInteractionContext context);

    protected virtual bool TryParseData<TMetadata>(IDiscordInteraction interaction, out SocketMessageComponent component, out TMetadata metadata) where TMetadata : IEmbedMetadata, new()
    {
        metadata = default;
        component = interaction as SocketMessageComponent;
        if (component == null) return false;

        var embed = component.Message.Embeds.FirstOrDefault();
        if (embed == null || embed.Footer == null || embed.Author == null)
            return false;

        return embed.TryParseMetadata(out metadata);
    }

    protected virtual int CheckNewPageNumber(int newPage, int maxPages)
    {
        if (newPage >= maxPages) return maxPages - 1;
        else if (newPage < 0) return 0;

        return newPage;
    }
}
