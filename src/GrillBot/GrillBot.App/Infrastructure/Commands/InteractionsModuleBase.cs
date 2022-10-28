using Discord.Interactions;
using GrillBot.App.Actions;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Infrastructure.Commands;

[DefaultMemberPermissions(GuildPermission.UseApplicationCommands)]
public abstract class InteractionsModuleBase : InteractionModuleBase<SocketInteractionContext>
{
    protected ITextsManager Texts { get; }
    protected IServiceProvider ServiceProvider { get; }
    protected GrillBotDatabaseBuilder DatabaseBuilder { get; }

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

    private bool IsBotRoom { get; set; }

    protected InteractionsModuleBase(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;

        if (ServiceProvider == null) return;
        Texts = ServiceProvider.GetRequiredService<ITextsManager>();
        DatabaseBuilder = ServiceProvider.GetRequiredService<GrillBotDatabaseBuilder>();
    }

    /// <summary>
    /// Check whether Defer can be performed.
    /// </summary>
    /// <returns>
    /// canDefer(False) if:
    /// - Called command is component command.
    /// - Contains "secret" or "tajne" parameter.
    /// - Contains DeferConfigurationAttribute with SuppressAuto=true
    ///
    /// ephemeral(True) if:
    /// - DeferConfigurationAttribute have RequireEphemeral=true
    /// - BotRoom was configured and execution channel is not BotRoom. 
    /// </returns>
    private async Task<(bool canDefer, bool ephemeral)> CheckDeferAsync(ICommandInfo command)
    {
        if (command.Module.ComponentCommands.Any(c => c.Name == command.Name)) return (false, false);
        if (command.Parameters.Any(c => c.Name is "secret" or "tajne")) return (false, false);

        var configuration = command.Attributes.OfType<DeferConfiguration>().FirstOrDefault() ?? new DeferConfiguration();

        if (configuration.SuppressAuto) return (false, false);
        if (configuration.RequireEphemeral) return (true, true);

        await using var repository = DatabaseBuilder.CreateRepository();
        var guild = (await repository.Guild.FindGuildAsync(Context.Guild, true))!;
        IsBotRoom = string.IsNullOrEmpty(guild.BotRoomChannelId) || guild.BotRoomChannelId == Context.Channel.Id.ToString();
        return (true, !IsBotRoom);
    }

    public override async Task BeforeExecuteAsync(ICommandInfo command)
    {
        await base.BeforeExecuteAsync(command);

        var (canDefer, ephemeral) = await CheckDeferAsync(command);
        if (canDefer) await DeferAsync(ephemeral);
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
        secret = secret || !IsBotRoom;

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

    protected ScopedCommand<TAction> GetActionAsCommand<TAction>() where TAction : ApiAction
    {
        var command = new ScopedCommand<TAction>(ServiceProvider.CreateScope());
        command.Command.UpdateContext(Locale, Context.User);

        return command;
    }
}
