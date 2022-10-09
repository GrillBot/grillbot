using Discord.Interactions;
using GrillBot.App.Actions;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Infrastructure.Commands;

[DefaultMemberPermissions(GuildPermission.UseApplicationCommands)]
public abstract class InteractionsModuleBase : InteractionModuleBase<SocketInteractionContext>
{
    protected bool CanDefer { get; set; } = true;
    protected ITextsManager Texts { get; }
    protected IServiceProvider ServiceProvider { get; }

    protected string Locale
    {
        get
        {
            var locale = Context?.Interaction?.UserLocale ?? "";
            return TextsManager.IsSupportedLocale(locale) ? locale : TextsManager.DefaultLocale;
        }
    }

    protected CultureInfo Culture
        => string.IsNullOrEmpty(Locale) ? null : Texts.GetCulture(Locale);

    protected InteractionsModuleBase(ITextsManager texts = null, IServiceProvider serviceProvider = null)
    {
        Texts = texts;
        ServiceProvider = serviceProvider;
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

    protected async Task<IUserMessage> SetResponseAsync(string content = null, Embed embed = default, Embed[] embeds = default, MessageComponent components = default, MessageFlags? flags = default,
        IEnumerable<FileAttachment> attachments = default, RequestOptions requestOptions = null, bool secret = false, bool suppressFollowUp = false)
    {
        var attachmentsList = (attachments ?? Enumerable.Empty<FileAttachment>()).ToList();

        if (!Context.Interaction.HasResponded)
        {
            if (attachmentsList.Count > 0)
                await RespondWithFilesAsync(attachments, content, embeds, false, secret, null, components, embed, requestOptions);
            else
                await RespondAsync(content, embeds, false, secret, null, requestOptions, components, embed);
            return await Context.Interaction.GetOriginalResponseAsync();
        }

        if (Context.Interaction.IsValidToken && !suppressFollowUp)
        {
            if (attachmentsList.Count > 0)
                return await FollowupWithFilesAsync(attachmentsList, content, embeds, false, secret, null, components, embed, requestOptions);
            return await FollowupAsync(content, embeds, false, secret, null, requestOptions, components, embed);
        }

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

    protected string GetText(string method, string id)
        => Texts?[GetTextId(method, id), Locale];

    protected string GetTextId(string method, string id) => $"{GetType().Name}/{method.Replace("Async", "")}/{id}";

    protected ScopedCommand<TCommand> GetCommand<TCommand>() where TCommand : CommandAction
    {
        var command = new ScopedCommand<TCommand>(ServiceProvider.CreateScope());
        command.Command.Init(Context);

        return command;
    }
}
