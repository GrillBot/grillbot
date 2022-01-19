using Discord.Interactions;

namespace GrillBot.App.Infrastructure;

public abstract class InteractionsModuleBase : InteractionModuleBase<SocketInteractionContext>
{
    protected async Task DeleteOriginalResponseAsync(RequestOptions options = null)
    {
        var response = await Context.Interaction.GetOriginalResponseAsync(options);
        if (response != null)
            await response.DeleteAsync(options);
    }

    protected Task<RestInteractionMessage> SetResponseAsync(string content = null, Embed embed = default, Embed[] embeds = default,
        MessageComponent components = default, MessageFlags? flags = default, IEnumerable<FileAttachment> attachments = default,
        RequestOptions requestOptions = null)
    {
        return Context.Interaction.ModifyOriginalResponseAsync(msg =>
        {
            msg.Components = components;
            msg.Flags = flags;
            msg.Attachments = attachments == default ? default : new Optional<IEnumerable<FileAttachment>>(attachments);
            msg.Content = content;
            msg.Embeds = embeds;
            msg.Embed = embed;
        }, requestOptions);
    }

    protected async Task<RestUserMessage> ReplyFileAsync(string filepath, bool spoiler, string text = null, Embed embed = null, bool noReply = false)
    {
        var originalMessage = await Context.Interaction.GetOriginalResponseAsync();
        var reference = !noReply ? new MessageReference(originalMessage.Id, Context.Channel.Id, Context.Guild.Id) : null;
        return await Context.Channel.SendFileAsync(filepath, text, false, embed, null, spoiler, new(AllowedMentionTypes.Users), reference);
    }

}
