using Discord.Interactions;
using GrillBot.Common.Managers;

namespace GrillBot.App.Infrastructure.Commands;

[DefaultMemberPermissions(GuildPermission.UseApplicationCommands)]
public abstract class InteractionsModuleBase : InteractionModuleBase<SocketInteractionContext>
{
    protected bool CanDefer { get; set; } = true;
    private LocalizationManager Localization { get; }

    protected string Locale
        => Context?.Interaction?.UserLocale;

    protected CultureInfo Culture
        => string.IsNullOrEmpty(Locale) ? null : Localization.GetCulture(Locale);

    protected InteractionsModuleBase(LocalizationManager localization = null)
    {
        Localization = localization;
    }

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

        if (command.Attributes.OfType<SuppressDeferAttribute>().Any())
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
        if (!Context.Interaction.HasResponded)
        {
            await RespondAsync(content, embeds, false, secret, null, requestOptions, components, embed);
            return await Context.Interaction.GetOriginalResponseAsync();
        }

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

    protected string GetLocale(string method, string id)
        => Localization?[GetLocaleId(method, id), Locale];

    protected string GetLocaleId(string method, string id) => $"{GetType().Name}/{method.Replace("Async", "")}/{id}";
}
