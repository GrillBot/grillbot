using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace GrillBot.App.Infrastructure
{
    public class ModuleBase : ModuleBase<SocketCommandContext>
    {
        protected MessageReference ReplyReference => new MessageReference(Context.Message.Id, Context.Channel.Id, Context.Guild?.Id);
        protected AllowedMentions AllowedMentions => new AllowedMentions() { MentionRepliedUser = true };

        protected Task<IUserMessage> ReplyAsync(string text = null, Embed embed = null) =>
            base.ReplyAsync(text, false, embed, null, AllowedMentions, ReplyReference);
    }
}
