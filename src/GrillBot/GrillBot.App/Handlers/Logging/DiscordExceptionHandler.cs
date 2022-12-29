using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using Discord.Net;
using GrillBot.App.Infrastructure.IO;
using GrillBot.App.Services.Images;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Exceptions;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Handlers.Logging;

public class DiscordExceptionHandler : ILoggingHandler
{
    private IDiscordClient DiscordClient { get; }
    private IConfiguration Configuration { get; }
    private ITextChannel LogChannel { get; set; }
    private IServiceProvider ServiceProvider { get; }

    public DiscordExceptionHandler(IDiscordClient discordClient, IConfiguration configuration, IServiceProvider serviceProvider)
    {
        DiscordClient = discordClient;
        Configuration = configuration.GetSection("Discord:Logging");
        ServiceProvider = serviceProvider;
    }

    public async Task<bool> CanHandleAsync(LogSeverity severity, string source, Exception exception = null)
    {
        if (exception == null || !Configuration.GetValue<bool>("Enabled")) return false;
        if (severity != LogSeverity.Critical && severity != LogSeverity.Error && severity != LogSeverity.Warning) return false;

        var isIgnoredException = IsIgnoredException(exception);
        if (LogChannel != null) return !isIgnoredException;

        var guild = await DiscordClient.GetGuildAsync(Configuration.GetValue<ulong>("GuildId"));
        if (guild == null) return false;

        var channel = await guild.GetTextChannelAsync(Configuration.GetValue<ulong>("ChannelId"));
        if (channel == null) return false;
        LogChannel = channel;

        return !isIgnoredException;
    }

    private static bool IsIgnoredException(Exception exception)
    {
        var cases = new[]
        {
            () => exception is GatewayReconnectException || exception.InnerException is GatewayReconnectException,
            () => exception.InnerException == null && exception.Message.StartsWith("Server missed last heartbeat", StringComparison.InvariantCultureIgnoreCase),
            () =>
            {
                return exception is TaskCanceledException or HttpRequestException &&
                       exception.InnerException is IOException { InnerException: SocketException se } &&
                       new[] { SocketError.TimedOut, SocketError.ConnectionAborted }.Contains(se.SocketErrorCode);
            },
            // 11 is magic constant represents error "Resource temporarily unavailable".
            () => exception is HttpRequestException && exception.InnerException is SocketException { ErrorCode: 11 },
            () => exception.InnerException is WebSocketException or WebSocketClosedException,
            () => exception is TaskCanceledException && exception.InnerException is null
        };

        return cases.Any(@case => @case());
    }

    public Task InfoAsync(string source, string message) => Task.CompletedTask;

    public Task WarningAsync(string source, string message, Exception exception = null)
        => ErrorAsync(source, message, exception!);

    public async Task ErrorAsync(string source, string message, Exception exception)
    {
        var (embed, withoutErrorsImage) = await CreateErrorDataAsync(source, message, exception);

        try
        {
            await StoreLastErrorDateAsync();
            await LogChannel!.SendFileAsync(withoutErrorsImage.Path, embed: embed);
        }
        finally
        {
            withoutErrorsImage.Dispose();
        }
    }

    private async Task<(Embed, TemporaryFile)> CreateErrorDataAsync(string source, string message, Exception exception)
    {
        var embed = new EmbedBuilder()
            .WithColor(Color.Red)
            .WithCurrentTimestamp()
            .WithFooter(DiscordClient.CurrentUser);

        switch (exception)
        {
            case ApiException apiException:
            {
                embed.WithTitle("Při zpracování požadavku na API došlo k chybě")
                    .AddField("Adresa", apiException.Path)
                    .AddField("Controller", apiException.ControllerInfo);

                if (apiException.LoggedUser != null)
                    embed.AddField("Přihlášený uživatel", apiException.LoggedUser.GetFullName());

                var msg = (!string.IsNullOrEmpty(message) ? message + "\n" : "") + exception.Message;
                embed.AddField("Obsah chyby", msg.Cut(EmbedFieldBuilder.MaxFieldValueLength));
                break;
            }
            default:
            {
                var msg = (!string.IsNullOrEmpty(message) ? message + "\n" : "") + exception.Message;
                var title = source == "App Commands" ? "Při provádění integrovaného příkazu došlo k chybě." : "Došlo k neočekávané chybě.";

                embed.WithTitle(title)
                    .AddField("Zdroj", source, true)
                    .AddField("Typ", exception.GetType().Name, true)
                    .AddField("Obsah chyby", msg.Trim());
                break;
            }
        }

        var withoutErrorsImage = await CreateWithoutErrorsImage(exception);
        embed.WithImageUrl($"attachment://{Path.GetFileName(withoutErrorsImage.Path)}");

        return (embed.Build(), withoutErrorsImage);
    }

    private async Task<TemporaryFile> CreateWithoutErrorsImage(Exception exception)
    {
        var user = exception.GetUser(DiscordClient);

        using var scope = ServiceProvider.CreateScope();
        var renderer = scope.ServiceProvider.GetRequiredService<WithoutAccidentRenderer>();

        return await renderer!.RenderAsync(user, null, null, null, null);
    }

    private async Task StoreLastErrorDateAsync()
    {
        using var scope = ServiceProvider.CreateScope();
        var dataCacheManager = scope.ServiceProvider.GetRequiredService<DataCacheManager>();
        
        await dataCacheManager.SetValueAsync("LastErrorDate", DateTime.Now.ToString("o"), DateTime.MaxValue);
    }
}
