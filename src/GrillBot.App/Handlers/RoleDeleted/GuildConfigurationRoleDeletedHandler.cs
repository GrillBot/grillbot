using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Events.Create;

namespace GrillBot.App.Handlers.RoleDeleted;

public class GuildConfigurationRoleDeletedHandler : IRoleDeletedEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    private readonly IRabbitPublisher _rabbitPublisher;

    public GuildConfigurationRoleDeletedHandler(GrillBotDatabaseBuilder databaseBuilder, IRabbitPublisher rabbitPublisher)
    {
        DatabaseBuilder = databaseBuilder;
        _rabbitPublisher = rabbitPublisher;
    }

    public async Task ProcessAsync(IRole role)
    {
        using var repository = DatabaseBuilder.CreateRepository();

        var guild = await repository.Guild.FindGuildAsync(role.Guild);
        if (guild is null)
            return;

        var log = new List<string>();
        ModifyAssociationRoleId(role, guild, log);

        if (log.Count == 0)
            return;

        await WriteToAuditLogAsync(role.Guild, log);
        await repository.CommitAsync();
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
                Source = $"Events.RoleDeleted.{nameof(GuildConfigurationRoleDeletedHandler)}",
                SourceAppName = "GrillBot"
            }
        };

        await _rabbitPublisher.PublishAsync(new CreateItemsMessage(logRequest));
    }
}
