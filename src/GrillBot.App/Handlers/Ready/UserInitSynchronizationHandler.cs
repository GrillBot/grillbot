using GrillBot.App.Managers;
using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Data.Models.API.AuditLog.Filters;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Handlers.Ready;

public class UserInitSynchronizationHandler : IReadyEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }

    public UserInitSynchronizationHandler(GrillBotDatabaseBuilder databaseBuilder, IDiscordClient discordClient)
    {
        DatabaseBuilder = databaseBuilder;
        DiscordClient = discordClient;
    }

    public async Task ProcessAsync()
    {
        await ProcessMemberSynchronizationAsync();
        await ProcessBotAdminAsync();
    }

    private async Task ProcessMemberSynchronizationAsync()
    {
        var guilds = await DiscordClient.GetGuildsAsync();

        await using var repository = DatabaseBuilder.CreateRepository();
        var users = await repository.GuildUser.GetAllUsersAsync();

        foreach (var guild in guilds)
        {
            var members = (await guild.GetUsersAsync()).ToDictionary(o => o.Id, o => o);
            var locales = new Dictionary<string, string>();
            var nicknames = new Dictionary<string, List<string>>();

            foreach (var user in users.Where(o => o.GuildId == guild.Id.ToString()))
            {
                user.User!.Status = UserStatus.Offline;
                if (!members.TryGetValue(user.UserId.ToUlong(), out var guildUser))
                    continue;

                user.Update(guildUser);
                if (string.IsNullOrEmpty(user.User.Language))
                {
                    if (locales.Count == 0)
                        locales = await FindUserLocalesAsync(repository, guild);
                    user.User.Language = locales.TryGetValue(user.UserId, out var locale) ? locale : null;
                }

                if (user.Nicknames.Count == 0)
                {
                    if (nicknames.Count == 0)
                        nicknames = await FindUserNicknamesAsync(repository, guild);
                    SetNicknames(nicknames, user);
                }
            }
        }

        await repository.CommitAsync();
    }

    private static async Task<Dictionary<string, string>> FindUserLocalesAsync(GrillBotRepository repository, IGuild guild)
    {
        var parameters = new AuditLogListParams
        {
            Sort = { Descending = true, OrderBy = "CreatedAt" },
            Types = new List<AuditLogItemType> { AuditLogItemType.InteractionCommand },
            GuildId = guild.Id.ToString(),
            Pagination = { Page = 0, PageSize = int.MaxValue }
        };

        var auditLogs = await repository.AuditLog.GetLogListAsync(parameters, parameters.Pagination, null);
        return auditLogs.Data
            .Where(o => !string.IsNullOrEmpty(o.ProcessedUserId))
            .GroupBy(o => o.ProcessedUserId!)
            .Select(o => new { o.Key, Data = JsonConvert.DeserializeObject<Data.Models.AuditLog.InteractionCommandExecuted>(o.First().Data)! })
            .Where(o => !string.IsNullOrEmpty(o.Data.Locale))
            .ToDictionary(o => o.Key, o => o.Data.Locale);
    }

    private static async Task<Dictionary<string, List<string>>> FindUserNicknamesAsync(GrillBotRepository repository, IGuild guild)
    {
        var parameters = new AuditLogListParams
        {
            Sort = { Descending = false, OrderBy = "CreatedAt" },
            Types = new List<AuditLogItemType> { AuditLogItemType.MemberUpdated },
            GuildId = guild.Id.ToString(),
            Pagination = { Page = 0, PageSize = int.MaxValue }
        };

        var auditLogs = await repository.AuditLog.GetLogListAsync(parameters, parameters.Pagination, null);
        return auditLogs.Data
            .Where(o => !string.IsNullOrEmpty(o.ProcessedUserId))
            .Select(o => JsonConvert.DeserializeObject<MemberUpdatedData>(o.Data, AuditLogWriteManager.SerializerSettings)!)
            .Where(o => o.Nickname != null && (!string.IsNullOrEmpty(o.Nickname.Before) || !string.IsNullOrEmpty(o.Nickname.After)))
            .GroupBy(o => !string.IsNullOrEmpty(o.Target.UserId) ? o.Target.UserId : o.Target.Id.ToString())
            .Select(o => new
            {
                o.Key,
                Data = o.Select(x => new[] { x.Nickname.Before, x.Nickname.After })
                    .SelectMany(x => x)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .ToList()
            })
            .ToDictionary(o => o.Key, o => o.Data);
    }

    private static void SetNicknames(IReadOnlyDictionary<string, List<string>> cachedNicknames, GuildUser user)
    {
        if (!cachedNicknames.TryGetValue(user.UserId, out var nicknames)) return;

        for (var i = 0; i < nicknames.Count; i++)
        {
            user.Nicknames.Add(new Nickname
            {
                GuildId = user.GuildId,
                UserId = user.UserId,
                NicknameValue = nicknames[i],
                Id = i + 1
            });
        }
    }

    private async Task ProcessBotAdminAsync()
    {
        var appInfo = await DiscordClient.GetApplicationInfoAsync();
        await using var repository = DatabaseBuilder.CreateRepository();

        var botOwner = await repository.User.GetOrCreateUserAsync(appInfo.Owner);
        botOwner.Flags |= (int)UserFlags.BotAdmin;
        botOwner.Flags &= ~(int)UserFlags.NotUser;

        await repository.CommitAsync();
    }
}
