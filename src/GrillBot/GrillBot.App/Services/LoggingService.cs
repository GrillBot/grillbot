using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using GrillBot.App.Extensions.Discord;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Database.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace GrillBot.App.Services
{
    public class LoggingService
    {
        private DiscordSocketClient DiscordClient { get; }
        private CommandService CommandService { get; }
        private ILoggerFactory LoggerFactory { get; }
        private IConfiguration Configuration { get; }
        private GrillBotContextFactory DbFactory { get; }

        public LoggingService(DiscordSocketClient discordSocketClient, CommandService commandService, ILoggerFactory loggerFactory, IConfiguration configuration,
            GrillBotContextFactory dbFactory)
        {
            DiscordClient = discordSocketClient;
            CommandService = commandService;
            LoggerFactory = loggerFactory;
            Configuration = configuration.GetSection("Discord:Logging");
            DbFactory = dbFactory;

            DiscordClient.Log += OnLogAsync;
            CommandService.Log += OnLogAsync;
        }

        public async Task OnLogAsync(LogMessage message)
        {
            SaveLog(message);
            await TryPostException(message);
        }

        private void SaveLog(LogMessage message)
        {
            try
            {
                var logger = LoggerFactory.CreateLogger(message.Source);

                switch (message.Severity)
                {
                    case LogSeverity.Critical:
                        logger.LogCritical(message.Exception, message.Message);
                        break;
                    case LogSeverity.Debug:
                        logger.LogDebug(message.Message);
                        break;
                    case LogSeverity.Error:
                        logger.LogError(message.Exception, message.Message);
                        break;
                    case LogSeverity.Warning when message.Exception != null:
                        logger.LogWarning(message.Exception, message.Message);
                        break;
                    case LogSeverity.Warning when message.Exception == null:
                        logger.LogWarning(message.Message);
                        break;
                    case LogSeverity.Info:
                    case LogSeverity.Verbose:
                        logger.LogInformation(message.Message);
                        break;
                }
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine(message.ToString());
                Console.WriteLine(ex.ToString());
            }
        }

        private async Task TryPostException(LogMessage message)
        {
            var guild = DiscordClient.GetGuild(Configuration.GetValue<ulong>("GuildId"));
            var channel = guild?.GetTextChannel(Configuration.GetValue<ulong>("ChannelId"));

            if (!CanSendException(message, guild, channel)) return;

            await StoreExceptionAsync(message);
            var embed = CreateErrorEmbed(message);
            await channel?.SendMessageAsync(embed: embed);
        }

        private bool CanSendException(LogMessage message, SocketGuild guild, ITextChannel channel)
        {
            var ex = message.Exception;

            // Invalid cases that says "This exception cannot be send to discord."
            var invalidCases = new Func<bool>[]
            {
                () => guild == null,
                () => channel == null,
                () => ex == null,
                () => !Configuration.GetValue<bool>("Enabled"),
                () => ex is GatewayReconnectException,
                () => ex.InnerException is GatewayReconnectException,
                () => ex.InnerException == null && ex.Message.StartsWith("Server missed last heartbeat", StringComparison.InvariantCultureIgnoreCase),
                () =>
                {
                    return (ex is TaskCanceledException || ex is HttpRequestException) &&
                    ex.InnerException is IOException ioe &&
                    ioe.InnerException is SocketException se &&
                    (new[]{ SocketError.TimedOut, SocketError.ConnectionAborted }).Contains(se.SocketErrorCode);
                },
                // 11 is magic constant represents error "Resource temporarily unavailable".
                () => ex is HttpRequestException && ex.InnerException is SocketException s && s.ErrorCode == 11,
                () => ex.InnerException != null && (ex.InnerException is WebSocketException || ex.InnerException is WebSocketClosedException)
            };

            return !invalidCases.Any(o => o());
        }

        private Embed CreateErrorEmbed(LogMessage message)
        {
            var embed = new EmbedBuilder()
                .WithColor(Color.Red)
                .WithCurrentTimestamp()
                .WithFooter(DiscordClient.CurrentUser);

            if (message.Exception is CommandException ce)
            {
                embed.WithTitle("Při provádění příkazu došlo k chybě")
                    .AddField("Kanál", ce.Context.Channel.GetMention(), true)
                    .AddField("Uživatel", ce.Context.User.Mention, true)
                    .AddField("Zpráva", $"```{(ce.Context.Message.Content.Length < DiscordConfig.MaxMessageSize ? ce.Context.Message.Content : $"{ce.Context.Message.Content[..^6]}...")}```", false)
                    .AddField("Skok na zprávu", ce.Context.Message.GetJumpUrl(), false);
            }
            else
            {
                embed.WithTitle("Došlo k neočekávané chybě.")
                    .AddField("Zdroj", message.Source, true)
                    .AddField("Typ", message.Exception?.GetType()?.Name, true)
                    .AddField("Zpráva chyby", message.Message ?? "", false);
            }

            return embed.Build();
        }

        private async Task StoreExceptionAsync(LogMessage message)
        {
            var logItem = new AuditLogItem()
            {
                CreatedAt = DateTime.Now,
                Data = message.ToString(padSource: 50, prependTimestamp: false),
                Type = AuditLogItemType.Error
            };

            using var dbContext = DbFactory.Create();

            await dbContext.AddAsync(logItem);
            await dbContext.SaveChangesAsync();
        }
    }
}
