using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog.Events;

public class MemberRolesUpdatedEvent : AuditEventBase
{
    private Cacheable<SocketGuildUser, ulong> Before { get; }
    private SocketGuildUser After { get; }
    private DateTime NextEventAt { get; }
    private GrillBotContextFactory DbFactory { get; }

    private SocketGuild Guild => After.Guild;
    public bool Finished { get; private set; }

    public MemberRolesUpdatedEvent(AuditLogService auditLogService, Cacheable<SocketGuildUser, ulong> before, SocketGuildUser after,
        DateTime nextEventAt, GrillBotContextFactory dbFactory) : base(auditLogService)
    {
        Before = before;
        After = after;
        DbFactory = dbFactory;
        NextEventAt = nextEventAt;
    }

    public override Task<bool> CanProcessAsync()
    {
        return Task.FromResult(
            Before.HasValue &&
            DateTime.Now >= NextEventAt &&
            !Before.Value.Roles.SequenceEqual(After.Roles)
        );
    }

    public override async Task ProcessAsync()
    {
        using var context = DbFactory.Create();
        await context.InitGuildAsync(Guild, CancellationToken.None);
        await context.InitUserAsync(After, CancellationToken.None);
        await context.InitGuildUserAsync(Guild, After, CancellationToken.None);

        var timeLimit = DateTime.Now.AddDays(-7);
        var auditLogIds = await AuditLogService.GetDiscordAuditLogIds(context, Guild, null, new[] { AuditLogItemType.MemberRoleUpdated }, timeLimit);
        var logs = (await Guild.GetAuditLogsAsync(100, actionType: ActionType.MemberRoleUpdated).FlattenAsync())
            .Where(o => !auditLogIds.Contains(o.Id) && ((MemberRoleAuditLogData)o.Data).Target.Id == After.Id);

        var logData = new Dictionary<ulong, Tuple<List<ulong>, MemberUpdatedData>>();
        foreach (var item in logs)
        {
            if (!logData.ContainsKey(item.User.Id))
                logData.Add(item.User.Id, new Tuple<List<ulong>, MemberUpdatedData>(new List<ulong>(), new MemberUpdatedData(new AuditUserInfo(After))));

            var logItem = logData[item.User.Id];
            logItem.Item1.Add(item.Id);
            logItem.Item2.Merge(item.Data as MemberRoleAuditLogData, Guild);
        }

        foreach (var logItem in logData)
        {
            var json = JsonConvert.SerializeObject(logItem.Value.Item2, AuditLogService.JsonSerializerSettings);
            var processedUser = Guild.GetUser(logItem.Key);
            var discordAuditLogId = string.Join(",", logItem.Value.Item1);
            var entity = AuditLogItem.Create(AuditLogItemType.MemberRoleUpdated, Guild, null, processedUser, json, discordAuditLogId);

            await context.InitUserAsync(processedUser, CancellationToken.None);
            await context.InitGuildUserAsync(Guild, processedUser, CancellationToken.None);
            await context.AddAsync(entity);
        }

        await context.SaveChangesAsync();
        Finished = true;
    }
}
