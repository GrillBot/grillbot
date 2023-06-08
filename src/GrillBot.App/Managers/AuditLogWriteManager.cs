using GrillBot.Data.Models.AuditLog;

namespace GrillBot.App.Managers;

public class AuditLogWriteManager
{
    public static JsonSerializerSettings SerializerSettings => new()
    {
        DefaultValueHandling = DefaultValueHandling.Ignore,
        Formatting = Formatting.None,
        NullValueHandling = NullValueHandling.Ignore
    };

    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public AuditLogWriteManager(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task StoreAsync(List<AuditLogDataWrapper> items)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        foreach (var item in items.Where(o => o.Guild != null).DistinctBy(o => o.Guild.Id))
            await repository.Guild.GetOrCreateGuildAsync(item.Guild);

        foreach (var item in items.Where(o => o.Guild != null && o.Channel != null).DistinctBy(o => o.Channel.Id))
        {
            var guildChannel = await item.Guild.GetChannelAsync(item.Channel.Id);
            if (guildChannel != null)
                await repository.Channel.GetOrCreateChannelAsync(guildChannel);
        }

        foreach (var item in items.Where(o => o.ProcessedUser != null).DistinctBy(o => o.ProcessedUser.Id))
        {
            await repository.User.GetOrCreateUserAsync(item.ProcessedUser);
            if (item.Guild == null) continue;

            var guildUser = item.ProcessedUser as IGuildUser ?? await item.Guild.GetUserAsync(item.ProcessedUser.Id);
            if (guildUser != null)
                await repository.GuildUser.GetOrCreateGuildUserAsync(guildUser);
        }

        await repository.AddCollectionAsync(items.Select(o => o.ToEntity(SerializerSettings)));
        await repository.CommitAsync();
    }
}
