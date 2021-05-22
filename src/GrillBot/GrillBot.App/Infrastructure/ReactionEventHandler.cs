using Discord;
using System.Threading.Tasks;

namespace GrillBot.App.Infrastructure
{
    public abstract class ReactionEventHandler
    {
        public virtual Task<bool> OnReactionAdded(IMessage message, IEmote emote, IUser user) => Task.FromResult(false);
        public virtual Task<bool> OnReactionRemoved(IMessage message, IEmote emote, IUser user) => Task.FromResult(false);
    }
}
