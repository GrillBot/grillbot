using Discord.Interactions;
using GrillBot.App.Actions;
using GrillBot.App.Managers.Auth;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Core.Managers.Performance;
using GrillBot.Core.RabbitMQ.V2.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Infrastructure;

[DefaultMemberPermissions(GuildPermission.UseApplicationCommands)]
public abstract class InteractionsModuleBase : InteractionModuleBase<SocketInteractionContext>
{
    protected ITextsManager Texts { get; }
    protected IServiceProvider ServiceProvider { get; }

    protected IGuild Guild => Context.Guild;
    protected ISocketMessageChannel Channel => Context.Channel;
    protected IUser User => Context.User;

    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ICounterManager CounterManager { get; }

    protected string Locale
    {
        get
        {
            var locale = Context?.Interaction?.UserLocale ?? "";
            return TextsManager.IsSupportedLocale(locale) ? locale : TextsManager.DefaultLocale;
        }
    }

    private bool IsEphemeralChannel { get; set; }

    protected InteractionsModuleBase(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        Texts = ServiceProvider.GetRequiredService<ITextsManager>();
        DatabaseBuilder = ServiceProvider.GetRequiredService<GrillBotDatabaseBuilder>();
        CounterManager = ServiceProvider.GetRequiredService<ICounterManager>();
    }

    /// <summary>
    /// Check whether Defer can be performed.
    /// </summary>
    /// <returns>
    /// <para>
    /// canDefer(False) if:
    /// - Called command is component command.
    /// - Contains "secret" or "tajne" parameter.
    /// - Contains DeferConfigurationAttribute with SuppressAuto=true
    /// </para>
    /// <para>
    /// ephemeral(True) if:
    /// - DeferConfigurationAttribute have RequireEphemeral=true
    /// - Channel was configured to execute command as ephemeral.
    /// </para>
    /// </returns>
    private async Task<(bool canDefer, bool ephemeral)> CheckDeferAsync(ICommandInfo command)
    {
        if (command.Module.ComponentCommands.Any(c => c.Name == command.Name)) return (false, false);
        if (command.Parameters.Any(c => c.Name is "secret" or "tajne")) return (false, false);

        var configuration = command.Attributes.OfType<DeferConfiguration>().FirstOrDefault() ?? new DeferConfiguration();

        if (configuration.SuppressAuto) return (false, false);
        if (configuration.RequireEphemeral) return (true, true);

        using var repository = DatabaseBuilder.CreateRepository();
        IsEphemeralChannel = await repository.Channel.IsChannelEphemeralAsync(Context.Guild, Context.Channel);
        return (true, IsEphemeralChannel);
    }

    public override async Task BeforeExecuteAsync(ICommandInfo command)
    {
        await base.BeforeExecuteAsync(command);

        var (canDefer, ephemeral) = await CheckDeferAsync(command);
        if (canDefer) await DeferAsync(ephemeral);
    }

    protected override async Task DeleteOriginalResponseAsync()
    {
        IUserMessage? userMessage;
        using (CounterManager.Create("Discord.API.Interactions"))
            userMessage = await GetOriginalResponseAsync();

        if (userMessage != null)
        {
            using (CounterManager.Create("Discord.API.Messages"))
                await userMessage.DeleteAsync();
        }
    }

    protected async Task<IUserMessage> SetResponseAsync(string? content = null, Embed? embed = null, Embed[]? embeds = null, MessageComponent? components = null, MessageFlags? flags = null,
        IEnumerable<FileAttachment>? attachments = null, RequestOptions? requestOptions = null, bool secret = false, bool suppressFollowUp = false, AllowedMentions? allowedMentions = null)
    {
        using (CounterManager.Create("Discord.API.Interactions"))
        {
            var attachmentsList = (attachments ?? Enumerable.Empty<FileAttachment>()).ToList();
            secret = secret || IsEphemeralChannel;

            if (!Context.Interaction.HasResponded)
            {
                if (attachmentsList.Count > 0)
                    await RespondWithFilesAsync(attachments, content, embeds, false, secret, allowedMentions, components, embed, requestOptions);
                else
                    await RespondAsync(content, embeds, false, secret, allowedMentions, requestOptions, components, embed);
                return await Context.Interaction.GetOriginalResponseAsync();
            }

            if (Context.Interaction.IsValidToken && !suppressFollowUp)
            {
                if (attachmentsList.Count > 0)
                    return await FollowupWithFilesAsync(attachmentsList, content, embeds, false, secret, allowedMentions, components, embed, requestOptions);
                return await FollowupAsync(content, embeds, false, secret, allowedMentions, requestOptions, components, embed);
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
                msg.AllowedMentions = allowedMentions;
            }, requestOptions);
        }
    }

    protected string GetText(string method, string id)
        => Texts[$"{GetType().Name}/{method.Replace("Async", "")}/{id}", Locale];

    protected ScopedCommand<TCommand> GetCommand<TCommand>() where TCommand : CommandAction
    {
        var command = new ScopedCommand<TCommand>(ServiceProvider.CreateScope());
        command.Command.Init(Context);

        return command;
    }

    protected async Task<ScopedCommand<TCommand>> GetCommandAsync<TCommand>() where TCommand : CommandAction
    {
        var command = new ScopedCommand<TCommand>(ServiceProvider.CreateScope());

        await InitializeCommandAsync(command);
        command.Command.Init(Context);

        return command;
    }

    protected ScopedCommand<TAction> GetActionAsCommand<TAction>() where TAction : ApiAction
    {
        var command = new ScopedCommand<TAction>(ServiceProvider.CreateScope());
        command.Command.UpdateContext(Locale, Context.User);

        return command;
    }

    protected async Task<ScopedCommand<TAction>> GetActionAsCommandAsync<TAction>() where TAction : ApiAction
    {
        var command = new ScopedCommand<TAction>(ServiceProvider.CreateScope());

        await InitializeCommandAsync(command);
        command.Command.UpdateContext(Locale, Context.User);

        return command;
    }

    private async Task InitializeCommandAsync<TCommand>(ScopedCommand<TCommand> command) where TCommand : notnull
    {
        var jwtToken = await command.Resolve<JwtTokenManager>()
           .CreateTokenForUserAsync(Context.User, Locale, "localhost", Context);

        if (!string.IsNullOrEmpty(jwtToken.AccessToken))
            command.Resolve<ICurrentUserProvider>().SetCustomToken(jwtToken.AccessToken);
    }

    protected async Task SendViaRabbitAsync<TPayload>(TPayload payload) where TPayload : IRabbitMessage
    {
        using var publisher = await GetActionAsCommandAsync<RabbitMQPublisherAction>();
        var currentUser = publisher.Resolve<ICurrentUserProvider>();

        publisher.Command.Init(null!, new object[] { payload }, currentUser);
        await publisher.Command.ProcessAsync();
    }
}
