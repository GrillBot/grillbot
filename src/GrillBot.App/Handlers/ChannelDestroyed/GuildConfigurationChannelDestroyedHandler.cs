using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Events.Create;
using System.Reflection;

namespace GrillBot.App.Handlers.ChannelDestroyed;

public class GuildConfigurationChannelDestroyedHandler : IChannelDestroyedEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    private readonly IRabbitMQPublisher _rabbitPublisher;

    public GuildConfigurationChannelDestroyedHandler(GrillBotDatabaseBuilder databaseBuilder, IRabbitMQPublisher rabbitPublisher)
    {
        DatabaseBuilder = databaseBuilder;
        _rabbitPublisher = rabbitPublisher;
    }

    public async Task ProcessAsync(IChannel channel)
    {
        if (channel is not IGuildChannel guildChannel)
            return;

        await using var repository = DatabaseBuilder.CreateRepository();

        var guild = await repository.Guild.FindGuildAsync(guildChannel.Guild);
        if (guild is null)
            return;

        var log = new List<string>();
        ResetProperty(guildChannel.Id, guild, nameof(guild.AdminChannelId), log);
        ResetProperty(guildChannel.Id, guild, nameof(guild.EmoteSuggestionChannelId), log);
        ResetProperty(guildChannel.Id, guild, nameof(guild.VoteChannelId), log);
        ResetProperty(guildChannel.Id, guild, nameof(guild.BotRoomChannelId), log);

        if (log.Count == 0)
            return;

        await WriteToAuditLogAsync(guildChannel.Guild, log);
        await repository.CommitAsync();
    }

    private async Task WriteToAuditLogAsync(IGuild guild, List<string> log)
    {
        var logRequest = new LogRequest(LogType.Info, DateTime.UtcNow, guild.Id.ToString())
        {
            LogMessage = new LogMessageRequest
            {
                Message = string.Join(Environment.NewLine, log),
                Severity = LogSeverity.Info,
                Source = $"Events.ChannelDestroyed.{nameof(GuildConfigurationChannelDestroyedHandler)}",
                SourceAppName = "GrillBot"
            }
        };

        await _rabbitPublisher.PublishAsync(new CreateItemsPayload(new() { logRequest }));
    }

    private static void ResetProperty(ulong expectedId, Database.Entity.Guild guild, string propertyName, List<string> log)
    {
        var guildType = guild.GetType();
        var property = guildType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property is null)
            return;

        var propertyValue = property.GetValue(guild)?.ToString();
        if (string.IsNullOrEmpty(propertyValue) || propertyValue != expectedId.ToString())
            return;

        log.Add($"Removed {propertyName} value. OldValue:{propertyValue}");
        property.SetValue(guild, null);
    }
}
