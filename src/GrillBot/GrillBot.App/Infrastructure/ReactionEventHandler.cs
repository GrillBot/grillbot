using GrillBot.App.Extensions.Discord;
using GrillBot.App.Infrastructure.Embeds;

namespace GrillBot.App.Infrastructure
{
    public abstract class ReactionEventHandler
    {
        public virtual Task<bool> OnReactionAddedAsync(IUserMessage message, IEmote emote, IUser user) => Task.FromResult(false);
        public virtual Task<bool> OnReactionRemovedAsync(IUserMessage message, IEmote emote, IUser user) => Task.FromResult(false);

        protected virtual bool TryGetEmbedAndMetadata<TMetadata>(IUserMessage message, IEmote reaction, out IEmbed embed, out TMetadata metadata) where TMetadata : IEmbedMetadata, new()
        {
            embed = message.Embeds.FirstOrDefault();
            metadata = default;

            if (embed == null || embed.Footer == null || embed.Author == null) return false;
            if (!Emojis.PaginationEmojis.Any(o => o.IsEqual(reaction))) return false;
            if (message.ReferencedMessage == null) return false;
            return embed.TryParseMetadata(out metadata);
        }

        protected virtual int GetPageNumber(int current, int maxPages, IEmote emote)
        {
            int newPage = current;
            if (emote.IsEqual(Emojis.MoveToFirst)) newPage = 0;
            else if (emote.IsEqual(Emojis.MoveToLast)) newPage = maxPages - 1;
            else if (emote.IsEqual(Emojis.MoveToNext)) newPage++;
            else if (emote.IsEqual(Emojis.MoveToPrev)) newPage--;

            if (newPage >= maxPages) return maxPages - 1;
            else if (newPage < 0) return 0;
            return newPage;
        }
    }
}
