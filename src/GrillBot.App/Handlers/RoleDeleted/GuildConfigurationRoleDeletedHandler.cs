using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Request.CreateItems;

namespace GrillBot.App.Handlers.RoleDeleted;

public class GuildConfigurationRoleDeletedHandler : AuditLogServiceHandler, IRoleDeletedEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public GuildConfigurationRoleDeletedHandler(IAuditLogServiceClient client, GrillBotDatabaseBuilder databaseBuilder) : base(client)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(IRole role)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var guild = await repository.Guild.FindGuildAsync(role.Guild);
        if (guild is null)
            return;

        var log = new List<string>();
        ModifyMutedRoleId(role, guild, log);

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

    private async Task WriteToAuditLogAsync(IGuild guild, List<string> log)
    {
        var request = CreateRequest(LogType.Info, guild);
        request.LogMessage = new LogMessageRequest
        {
            Message = string.Join(Environment.NewLine, log),
            Severity = LogSeverity.Info,
            Source = $"Events.RoleDeleted.{nameof(GuildConfigurationRoleDeletedHandler)}",
            SourceAppName = "GrillBot"
        };

        await SendRequestAsync(request);
    }
}
