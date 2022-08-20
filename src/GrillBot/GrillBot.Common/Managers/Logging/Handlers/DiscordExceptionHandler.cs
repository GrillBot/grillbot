using System.Net.Sockets;
using System.Net.WebSockets;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using GrillBot.Common.Extensions.Discord;
using Microsoft.Extensions.Configuration;

namespace GrillBot.Common.Managers.Logging.Handlers;

public class DiscordExceptionHandler : ILoggingHandler
{
    private IDiscordClient DiscordClient { get; }
    private IConfiguration Configuration { get; }
    private ITextChannel? LogChannel { get; set; }

    public DiscordExceptionHandler(IDiscordClient discordClient, IConfiguration configuration)
    {
        DiscordClient = discordClient;
        Configuration = configuration.GetSection("Discord:Logging");
    }

    public async Task<bool> CanHandleAsync(LogSeverity severity, string source, Exception? exception = null)
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
        var cases = new Func<bool>[]
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

    public Task WarningAsync(string source, string message, Exception? exception = null)
        => ErrorAsync(source, message, exception!);

    public async Task ErrorAsync(string source, string message, Exception exception)
    {
        var embed = CreateErrorEmbed(source, message, exception);
        await LogChannel!.SendMessageAsync(embed: embed);
    }

    private Embed CreateErrorEmbed(string source, string? message, Exception exception)
    {
        var embed = new EmbedBuilder()
            .WithColor(Color.Red)
            .WithCurrentTimestamp()
            .WithFooter(DiscordClient.CurrentUser);

        if (exception is CommandException ce)
        {
            embed.WithTitle("Při provádění příkazu došlo k chybě")
                .AddField("Kanál", ce.Context.Channel.GetMention(), true)
                .AddField("Uživatel", ce.Context.User.Mention, true)
                .AddField("Zpráva", $"```{(ce.Context.Message.Content.Length < DiscordConfig.MaxMessageSize ? ce.Context.Message.Content : $"{ce.Context.Message.Content[..^6]}...")}```")
                .AddField("Skok na zprávu", ce.Context.Message.GetJumpUrl());
        }
        else
        {
            var msg = (!string.IsNullOrEmpty(message) ? message + "\n" : "") + exception.Message;
            var title = source == "App Commands" ? "Při provádění integrovaného příkazu došlo k chybě." : "Došlo k neočekávané chybě.";

            embed.WithTitle(title)
                .AddField("Zdroj", source, true)
                .AddField("Typ", exception.GetType().Name, true)
                .AddField("Zpráva chyby", msg.Trim());
        }

        return embed.Build();
    }
}
