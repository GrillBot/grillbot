using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Data.Models.API.AuditLog.Filters;
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
            var locales = await FindUserLocalesAsync(repository, guild);

            foreach (var user in users.Where(o => o.GuildId == guild.Id.ToString()))
            {
                user.User!.Status = UserStatus.Offline;
                if (!members.TryGetValue(user.UserId.ToUlong(), out var guildUser))
                    continue;

                user.Update(guildUser);
                if (string.IsNullOrEmpty(user.User.Language))
                    user.User.Language = locales.TryGetValue(user.UserId, out var locale) ? locale : null;
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
