using GrillBot.App.Managers;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Data.Models.API.AuditLog.Filters;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Handlers.UserJoined;

public class UserJoinedSyncHandler : IUserJoinedEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public UserJoinedSyncHandler(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(IGuildUser user)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var guildUser = await repository.GuildUser.FindGuildUserAsync(user);
        if (guildUser == null) return;

        await UpdateUserLanguageAsync(repository, guildUser);
        await UpdateNicknamesAsync(repository, user);

        await repository.CommitAsync();
    }

    private static async Task UpdateUserLanguageAsync(GrillBotRepository repository, Database.Entity.GuildUser guildUser)
    {
        if (!string.IsNullOrEmpty(guildUser.User!.Language)) return;

        var auditLogParams = new AuditLogListParams
        {
            Sort = { Descending = true, OrderBy = "CreatedAt" },
            Types = new List<AuditLogItemType> { AuditLogItemType.InteractionCommand },
            GuildId = guildUser.GuildId,
            ProcessedUserIds = new List<string> { guildUser.UserId }
        };

        var auditLogs = await repository.AuditLog.GetSimpleDataAsync(auditLogParams);
        guildUser.User.Language = auditLogs
            .Select(o => JsonConvert.DeserializeObject<Data.Models.AuditLog.InteractionCommandExecuted>(o.Data, AuditLogWriteManager.SerializerSettings)!)
            .FirstOrDefault(o => !string.IsNullOrEmpty(o.Locale))?.Locale;
    }

    private static async Task UpdateNicknamesAsync(GrillBotRepository repository, IGuildUser user)
    {
        if (await repository.Nickname.ExistsAnyNickname(user)) return;

        var auditLogParams = new AuditLogListParams
        {
            Sort = { Descending = true, OrderBy = "CreatedBy" },
            Types = new List<AuditLogItemType> { AuditLogItemType.MemberUpdated },
            GuildId = user.GuildId.ToString(),
            Pagination = { Page = 0, PageSize = int.MaxValue }
        };

        var auditLogs = await repository.AuditLog.GetLogListAsync(auditLogParams, auditLogParams.Pagination, null);
        var nicknames = auditLogs.Data
            .Where(o => !string.IsNullOrEmpty(o.ProcessedUserId))
            .Select(o => JsonConvert.DeserializeObject<MemberUpdatedData>(o.Data, AuditLogWriteManager.SerializerSettings)!)
            .Where(o => o.Target.UserId == user.Id.ToString() || o.Target.Id == user.Id)
            .Where(o => o.Nickname != null && (!string.IsNullOrEmpty(o.Nickname.Before) || !string.IsNullOrEmpty(o.Nickname.After)))
            .SelectMany(o => new[] { o.Nickname.Before, o.Nickname.After })
            .Where(o => !string.IsNullOrEmpty(o))
            .Distinct()
            .ToList();

        for (var i = 0; i < nicknames.Count; i++)
        {
            var entity = new Database.Entity.Nickname
            {
                Id = i + 1,
                UserId = user.Id.ToString(),
                GuildId = user.GuildId.ToString(),
                NicknameValue = nicknames[i]
            };

            await repository.AddAsync(entity);
        }
    }
}
