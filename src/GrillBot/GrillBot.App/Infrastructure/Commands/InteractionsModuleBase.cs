using Discord;
using Discord.Interactions;
using Discord.Rest;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrillBot.App.Infrastructure;

public abstract class InteractionsModuleBase : InteractionModuleBase<SocketInteractionContext>
{
    public async Task DeleteOriginalResponseAsync(RequestOptions options = null)
    {
        var response = await Context.Interaction.GetOriginalResponseAsync(options);
        if (response != null)
            await response.DeleteAsync(options);
    }

    public Task<RestInteractionMessage> SetResponseAsync(string content = null, Embed embed = default, Embed[] embeds = default,
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

}
