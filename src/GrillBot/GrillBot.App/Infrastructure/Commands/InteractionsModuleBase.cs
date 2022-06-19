using Discord.Interactions;

namespace GrillBot.App.Infrastructure.Commands;

[DefaultMemberPermissions(GuildPermission.UseApplicationCommands)]
public abstract class InteractionsModuleBase : InteractionModuleBase<SocketInteractionContext>
{
    protected bool CanDefer { get; set; } = true;

    /// <summary>
    /// Check whether Defer can be performed.
    /// </summary>
    /// <returns>
    /// True if called command is not component command and command not contains secret switch named "secret" or "tajne".
    /// In other case, it decides CanDefer parameter.
    /// </returns>
    private bool CheckDefer(ICommandInfo command)
    {
        if (command.Module.ComponentCommands.Any(c => c.Name == command.Name))
            return false;

        return !command.Parameters.Any(c => c.Name is "secret" or "tajne") && CanDefer;
    }

    public override async Task BeforeExecuteAsync(ICommandInfo command)
    {
        await base.BeforeExecuteAsync(command);

        if (CheckDefer(command))
            await DeferAsync();
    }

    protected override async Task DeleteOriginalResponseAsync()
    {
        var response = await GetOriginalResponseAsync();
        if (response != null)
            await response.DeleteAsync();
    }

    protected async Task<IUserMessage> SetResponseAsync(string content = null, Embed embed = default, Embed[] embeds = default,
        MessageComponent components = default, MessageFlags? flags = default, IEnumerable<FileAttachment> attachments = default,
        RequestOptions requestOptions = null, bool secret = false)
    {
        if (Context.Interaction.IsValidToken)
            return await FollowupAsync(content, embeds, false, secret, null, requestOptions, components, embed);

        if (secret)
            flags |= MessageFlags.Ephemeral;

        return await Context.Interaction.ModifyOriginalResponseAsync(msg =>
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
