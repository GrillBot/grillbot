using Discord.Interactions;
using GrillBot.App.Infrastructure.Jobs;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Exceptions;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Logging;
using GrillBot.Core.Extensions;
using GrillBot.Core.IO;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Handlers.Logging;

public class DiscordExceptionHandler : ILoggingHandler
{
    private IDiscordClient DiscordClient { get; }
    private IConfiguration Configuration { get; }
    private IServiceProvider ServiceProvider { get; }

    private ITextChannel? LogChannel { get; set; }

    public DiscordExceptionHandler(IDiscordClient discordClient, IConfiguration configuration, IServiceProvider serviceProvider)
    {
        DiscordClient = discordClient;
        Configuration = configuration.GetSection("Discord:Logging");
        ServiceProvider = serviceProvider;
    }

    public async Task<bool> CanHandleAsync(LogSeverity severity, string source, Exception? exception = null)
    {
        if (exception is null || !Configuration.GetValue<bool>("Enabled")) return false;
        if (severity != LogSeverity.Critical && severity != LogSeverity.Error && severity != LogSeverity.Warning) return false;

        var ex = exception is InteractionException ? exception.InnerException! : exception;
        var isWarning = LoggingHelper.IsWarning(source, ex);
        if (LogChannel != null) return !isWarning;

        var guild = await DiscordClient.GetGuildAsync(Configuration.GetValue<ulong>("GuildId"));
        if (guild == null) return false;

        var channel = await guild.GetTextChannelAsync(Configuration.GetValue<ulong>("ChannelId"));
        if (channel == null) return false;
        LogChannel = channel;

        return !isWarning;
    }

    public Task InfoAsync(string source, string message) => Task.CompletedTask;

    public Task WarningAsync(string source, string message, Exception? exception = null)
        => ErrorAsync(source, message, exception!);

    public async Task ErrorAsync(string source, string message, Exception exception)
    {
        if (LogChannel is null)
            return;

        var (embed, withoutErrorsImage) = await CreateErrorDataAsync(source, message, exception);

        try
        {
            await StoreLastErrorDateAsync();
            if (withoutErrorsImage is null)
                await LogChannel.SendMessageAsync(embed: embed);
            else
                await LogChannel.SendFileAsync(withoutErrorsImage.Path, embed: embed);
        }
        finally
        {
            withoutErrorsImage?.Dispose();
        }
    }

    private async Task<(Embed, TemporaryFile?)> CreateErrorDataAsync(string source, string message, Exception exception)
    {
        var embed = new EmbedBuilder()
            .WithColor(Color.Red)
            .WithCurrentTimestamp()
            .WithFooter(DiscordClient.CurrentUser);

        switch (exception)
        {
            case ApiException apiException:
                {
                    embed.WithTitle("Při zpracování požadavku na API došlo k chybě");
                    embed.AddField("Adresa", apiException.Path);
                    embed.AddField("Controller", apiException.ControllerInfo);

                    if (apiException.LoggedUser != null)
                        embed.AddField("Přihlášený uživatel", apiException.LoggedUser.GetFullName());

                    var msg = (!string.IsNullOrEmpty(message) ? message + "\n" : "") + exception.Message;
                    embed.AddField("Obsah chyby", msg.Cut(EmbedFieldBuilder.MaxFieldValueLength));
                    break;
                }
            case InteractionException interactionException:
                {
                    var cmd = interactionException.CommandInfo;
                    var context = interactionException.InteractionContext;

                    embed.WithTitle("Při provádění příkazu došlo k chybě.");

                    if (context.Guild is not null)
                        embed.AddField("Server", context.Guild.Name, true);

                    embed.AddField("Kanál", context.Channel.Name, true);
                    embed.AddField("Uživatel", context.User.GetFullName());
                    embed.AddField("Příkaz", $"{cmd.Name} ({cmd.Module}/{cmd.MethodName})");

                    exception = interactionException.InnerException!;
                    var msg = (!string.IsNullOrEmpty(message) ? message + "\n" : "") + exception.Message;
                    embed.AddField("Obsah chyby", msg.Trim().Cut(EmbedFieldBuilder.MaxFieldValueLength));

                    break;
                }
            default:
                {
                    var msg = (!string.IsNullOrEmpty(message) ? message + "\n" : "") + exception.Message;
                    var title = "Došlo k neočekávané chybě.";

                    if (exception is JobException jobException)
                    {
                        if (jobException.LoggedUser is not null)
                            embed.AddField("Spustil", jobException.LoggedUser.GetFullName(), true);

                        title = "Při běhu naplánované úlohy došlo k chybě.";
                        exception = exception.InnerException!;
                        msg = (!string.IsNullOrEmpty(message) ? message + "\n" : "") + exception.Message;
                    }

                    embed.WithTitle(title)
                        .AddField("Zdroj", source, true)
                        .AddField("Typ", exception.GetType().Name, true)
                        .AddField("Obsah chyby", msg.Trim().Cut(EmbedFieldBuilder.MaxFieldValueLength));
                    break;
                }
        }

        var withoutErrorsImage = await CreateWithoutErrorsImage(exception);
        if (withoutErrorsImage is not null)
            embed.WithImageUrl($"attachment://{Path.GetFileName(withoutErrorsImage.Path)}");

        return (embed.Build(), withoutErrorsImage);
    }

    private async Task<TemporaryFile?> CreateWithoutErrorsImage(Exception exception)
    {
        try
        {
            var user = exception is JobException jobException ? jobException.LoggedUser : exception.GetUser();
            user ??= DiscordClient.CurrentUser;

            using var scope = ServiceProvider.CreateScope();
            var renderer = scope.ServiceProvider.GetRequiredService<WithoutAccidentRenderer>();

            return await renderer.RenderAsync(user);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async Task StoreLastErrorDateAsync()
    {
        using var scope = ServiceProvider.CreateScope();
        var dataCacheManager = scope.ServiceProvider.GetRequiredService<DataCacheManager>();

        await dataCacheManager.SetValueAsync("LastErrorDate", DateTime.Now.ToString("o"), DateTime.MaxValue);
    }
}
