using Discord;
using Discord.Commands;
using Discord.Rest;
using System.IO;
using System.Threading.Tasks;

namespace GrillBot.App.Infrastructure
{
    public class ModuleBase : ModuleBase<SocketCommandContext>
    {
        protected MessageReference ReplyReference => new MessageReference(Context.Message.Id, Context.Channel.Id, Context.Guild?.Id);
        protected AllowedMentions AllowedMentions => new AllowedMentions() { MentionRepliedUser = true };

        protected Task<IUserMessage> ReplyAsync(string text = null, Embed embed = null) =>
            base.ReplyAsync(text, false, embed, null, AllowedMentions, ReplyReference);

        protected Task<RestUserMessage> ReplyStreamAsync(Stream stream, string filename, bool spoiler, string text = null, Embed embed = null) =>
            Context.Channel.SendFileAsync(stream, filename, text, false, embed, null, spoiler, AllowedMentions, ReplyReference);
    }
}
