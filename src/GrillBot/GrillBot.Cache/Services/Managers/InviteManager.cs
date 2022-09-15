using Discord;
using GrillBot.Cache.Entity;
using GrillBot.Common.Managers.Counters;

namespace GrillBot.Cache.Services.Managers;

public class InviteManager
{
    private SemaphoreSlim Semaphore { get; }
    private GrillBotCacheBuilder CacheBuilder { get; }
    private CounterManager CounterManager { get; }

    public InviteManager(GrillBotCacheBuilder cacheBuilder, CounterManager counterManager)
    {
        CacheBuilder = cacheBuilder;
        Semaphore = new SemaphoreSlim(1);
        CounterManager = counterManager;
    }

    public async Task<List<IInviteMetadata>> DownloadInvitesAsync(IGuild guild)
    {
        var result = new List<IInviteMetadata>();

        using (CounterManager.Create("Discord.API"))
        {
            if (!string.IsNullOrEmpty(guild.VanityURLCode))
                result.Add(await guild.GetVanityInviteAsync());

            result.AddRange(await guild.GetInvitesAsync());
        }

        return result;
    }

    public async Task UpdateMetadataAsync(IGuild guild, IEnumerable<IInviteMetadata> invites)
    {
        await Semaphore.WaitAsync();

        try
        {
            await using var cache = CacheBuilder.CreateRepository();

            var guildInvites = await cache.InviteMetadataRepository.GetInvitesOfGuildAsync(guild);
            cache.RemoveCollection(guildInvites);

            foreach (var invite in invites.Select(ConvertMetadata).Where(o => o != null))
                await cache.AddAsync(invite!);

            await cache.CommitAsync();
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async Task<int> GetMetadataCountAsync()
    {
        await Semaphore.WaitAsync();

        try
        {
            await using var cache = CacheBuilder.CreateRepository();
            return await cache.InviteMetadataRepository.GetCountAsync();
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async Task AddInviteAsync(IInviteMetadata metadata)
    {
        await Semaphore.WaitAsync();

        try
        {
            var entity = ConvertMetadata(metadata);
            if (entity == null) return;

            await using var cache = CacheBuilder.CreateRepository();
            if (await cache.InviteMetadataRepository.InviteExistsAsync(metadata.Guild, metadata))
                return;

            await cache.AddAsync(entity);
            await cache.CommitAsync();
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public static InviteMetadata? ConvertMetadata(IInviteMetadata invite)
    {
        if (invite.GuildId == null)
            return null;

        var createdAt = invite.CreatedAt?.LocalDateTime;

        return new InviteMetadata
        {
            Code = invite.Code,
            Uses = invite.Uses ?? 0,
            CreatedAt = createdAt == DateTime.MinValue ? null : createdAt,
            CreatorId = invite.Inviter?.Id.ToString(),
            GuildId = invite.GuildId.ToString()!,
            IsVanity = invite.Guild.VanityURLCode == invite.Code
        };
    }

    public async Task<List<InviteMetadata>> GetInvitesAsync(IGuild guild)
    {
        await Semaphore.WaitAsync();

        try
        {
            await using var cache = CacheBuilder.CreateRepository();
            return await cache.InviteMetadataRepository.GetInvitesOfGuildAsync(guild);
        }
        finally
        {
            Semaphore.Release();
        }
    }
}
