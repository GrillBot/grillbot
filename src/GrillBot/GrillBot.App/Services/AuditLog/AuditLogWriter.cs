using GrillBot.Data.Models.AuditLog;

namespace GrillBot.App.Services.AuditLog;

public class AuditLogWriter
{
    public static JsonSerializerSettings SerializerSettings => new()
    {
        DefaultValueHandling = DefaultValueHandling.Ignore,
        Formatting = Formatting.None,
        NullValueHandling = NullValueHandling.Ignore
    };

    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public AuditLogWriter(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public Task StoreAsync(AuditLogDataWrapper item)
        => StoreAsync(new List<AuditLogDataWrapper> { item });

    public async Task StoreAsync(List<AuditLogDataWrapper> items)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        foreach (var item in items.Where(o => o.Guild != null).DistinctBy(o => o.Guild.Id))
            await repository.Guild.GetOrCreateRepositoryAsync(item.Guild);

        foreach (var item in items.Where(o => o.Guild != null && o.Channel != null).DistinctBy(o => o.Channel.Id))
            await repository.Channel.GetOrCreateChannelAsync(item.Channel);

        foreach (var item in items.Where(o => o.ProcessedUser != null).DistinctBy(o => o.ProcessedUser.Id))
        {
            if (item.Guild != null)
            {
                var guildUser = item.ProcessedUser as IGuildUser ?? await item.Guild.GetUserAsync(item.ProcessedUser.Id);
                if (guildUser != null)
                    await repository.GuildUser.GetOrCreateGuildUserAsync(guildUser);
            }
            else
            {
                await repository.User.GetOrCreateUserAsync(item.ProcessedUser);
            }
        }

        await repository.AddCollectionAsync(items.Select(o => o.ToEntity(SerializerSettings)));
        await repository.CommitAsync();
    }
}
