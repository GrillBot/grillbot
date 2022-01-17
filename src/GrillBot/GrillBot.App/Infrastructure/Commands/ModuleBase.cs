﻿using Discord;
using Discord.Commands;
using Discord.Rest;
using System.IO;
using System.Threading.Tasks;

namespace GrillBot.Data.Infrastructure
{
    public class ModuleBase : ModuleBase<SocketCommandContext>
    {
        protected MessageReference ReplyReference => new(Context.Message.Id, Context.Channel.Id, Context.Guild?.Id);
        protected AllowedMentions AllowedMentions => new() { MentionRepliedUser = true };

        protected Task<IUserMessage> ReplyAsync(string text = null, Embed embed = null) =>
            base.ReplyAsync(text, false, embed, null, AllowedMentions, ReplyReference);

        protected Task<RestUserMessage> ReplyStreamAsync(Stream stream, string filename, bool spoiler, string text = null, Embed embed = null) =>
            Context.Channel.SendFileAsync(stream, filename, text, false, embed, null, spoiler, AllowedMentions, ReplyReference);

        protected Task<RestUserMessage> ReplyFileAsync(string filepath, bool spoiler, string text = null, Embed embed = null) =>
            Context.Channel.SendFileAsync(filepath, text, false, embed, null, spoiler, AllowedMentions, ReplyReference);
    }
}
