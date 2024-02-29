using AuditLogService.Models.Events.Create;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Database.Enums;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Handlers.RoleDeleted;

public class GuildConfigurationRoleDeletedHandler : IRoleDeletedEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    private readonly IRabbitMQPublisher _rabbitPublisher;

    public GuildConfigurationRoleDeletedHandler(GrillBotDatabaseBuilder databaseBuilder, IRabbitMQPublisher rabbitPublisher)
    {
        DatabaseBuilder = databaseBuilder;
        _rabbitPublisher = rabbitPublisher;
    }

    public async Task ProcessAsync(IRole role)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var guild = await repository.Guild.FindGuildAsync(role.Guild);
        if (guild is null)
            return;

        var log = new List<string>();
        ModifyMutedRoleId(role, guild, log);
        ModifyAssociationRoleId(role, guild, log);

        if (log.Count == 0)
            return;

        await WriteToAuditLogAsync(role.Guild, log);
        await repository.CommitAsync();
    }

    private static void ModifyMutedRoleId(IRole role, Database.Entity.Guild guild, List<string> log)
    {
        if (string.IsNullOrEmpty(guild.MuteRoleId) || guild.MuteRoleId != role.Id.ToString())
            return;

        log.Add($"Removed MutedRoleId value. OldValue: {guild.MuteRoleId}");
        guild.MuteRoleId = null;
    }

    private static void ModifyAssociationRoleId(IRole role, Database.Entity.Guild guild, List<string> log)
    {
        if (string.IsNullOrEmpty(guild.AssociationRoleId) || guild.AssociationRoleId != role.Id.ToString())
            return;

        log.Add($"Removed AssociationRoleId value and cleared user flags. OldValue: {guild.AssociationRoleId}");
        guild.AssociationRoleId = null;
    }

    private async Task WriteToAuditLogAsync(IGuild guild, List<string> log)
    {
        var logRequest = new LogRequest(LogType.Info, DateTime.UtcNow, guild.Id.ToString())
        {
            LogMessage = new LogMessageRequest
            {
                Message = string.Join(Environment.NewLine, log),
                Severity = LogSeverity.Info,
                Source = $"Events.RoleDeleted.{nameof(GuildConfigurationRoleDeletedHandler)}",
                SourceAppName = "GrillBot"
            }
        };

        await _rabbitPublisher.PublishAsync(new CreateItemsPayload(new() { logRequest }));
    }
}
