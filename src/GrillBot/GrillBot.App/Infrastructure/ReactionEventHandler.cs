using Discord;
using System.Threading.Tasks;

namespace GrillBot.App.Infrastructure
{
    public abstract class ReactionEventHandler
    {
        public virtual Task<bool> OnReactionAddedAsync(IUserMessage message, IEmote emote, IUser user) => Task.FromResult(false);
        public virtual Task<bool> OnReactionRemovedAsync(IUserMessage message, IEmote emote, IUser user) => Task.FromResult(false);
    }
}
