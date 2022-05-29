using GrillBot.Database.Entity;

namespace GrillBot.App.Services.Discord.Synchronization;

public class ChannelSynchronization : SynchronizationBase
{
    public ChannelSynchronization(GrillBotDatabaseFactory dbFactory) : base(dbFactory)
    {
    }

    private static IQueryable<GuildChannel> GetBaseQuery(GrillBotContext context, ulong guildId)
        => context.Channels.Where(o => o.GuildId == guildId.ToString());

    public async Task ChannelDeletedAsync(ITextChannel channel)
    {
        using var context = DbFactory.Create();

        var baseQuery = GetBaseQuery(context, channel.GuildId);

        var dbChannel = await baseQuery.FirstOrDefaultAsync(o => o.ChannelId == channel.Id.ToString());
        if (dbChannel == null) return;

        dbChannel.MarkDeleted(true);
        if (channel is not IThreadChannel)
        {
            var threads = await baseQuery.Where(o => o.ParentChannelId == channel.Id.ToString()).ToListAsync();
            threads.ForEach(o => o.MarkDeleted(true));
        }

        await context.SaveChangesAsync();
    }

    public async Task ThreadDeletedAsync(IThreadChannel threadChannel)
    {
        using var context = DbFactory.Create();

        var baseQuery = GetBaseQuery(context, threadChannel.GuildId);
        var thread = await baseQuery.FirstOrDefaultAsync(o =>
            (o.ChannelType == ChannelType.PublicThread || o.ChannelType == ChannelType.PrivateThread) &&
            o.ChannelId == threadChannel.Id.ToString() && o.ParentChannelId == threadChannel.CategoryId.ToString()
        );

        if (thread == null) return;

        thread.MarkDeleted(true);
        await context.SaveChangesAsync();
    }

    public async Task ThreadUpdatedAsync(IThreadChannel before, IThreadChannel after)
    {
        if (before.Name == after.Name && before.IsArchived == after.IsArchived) return;

        using var context = DbFactory.Create();

        var baseQuery = GetBaseQuery(context, after.GuildId);
        var thread = await baseQuery.FirstOrDefaultAsync(o =>
            (o.ChannelType == ChannelType.PublicThread || o.ChannelType == ChannelType.PrivateThread) &&
            o.ChannelId == after.Id.ToString() && o.ParentChannelId == after.CategoryId.ToString()
        );
        if (thread == null) return;

        thread.Name = after.Name;
        thread.MarkDeleted(after.IsArchived);

        await context.SaveChangesAsync();
    }

    public async Task ChannelUpdatedAsync(ITextChannel before, ITextChannel after)
    {
        using var context = DbFactory.Create();

        var baseQuery = GetBaseQuery(context, before.GuildId);
        var channel = await baseQuery.FirstOrDefaultAsync(o => o.ChannelId == before.Id.ToString());
        if (channel == null) return;

        channel.MarkDeleted(false);
        channel.Name = after.Name;

        await context.SaveChangesAsync();
    }

    public async Task InitChannelsAsync(IGuild guild, List<GuildChannel> databaseChannels)
    {
        var guildChannels = databaseChannels.Where(o => o.GuildId == guild.Id.ToString());

        foreach (var textChannel in await guild.GetTextChannelsAsync())
        {
            var channel = guildChannels.FirstOrDefault(o => o.IsText() && o.ChannelId == textChannel.Id.ToString());
            if (channel == null) continue;

            channel.Name = textChannel.Name;
            channel.MarkDeleted(false);
        }

        foreach (var voiceChannel in await guild.GetVoiceChannelsAsync())
        {
            var channel = guildChannels.FirstOrDefault(o => o.IsVoice() && o.ChannelId == voiceChannel.Id.ToString());
            if (channel == null) continue;

            channel.Name = voiceChannel.Name;
            channel.MarkDeleted(false);
        }

        foreach (var stageChannel in await guild.GetStageChannelsAsync())
        {
            var channel = guildChannels.FirstOrDefault(o => o.IsStage() && o.ChannelId == stageChannel.Id.ToString());
            if (channel == null) continue;

            channel.Name = stageChannel.Name;
            channel.MarkDeleted(false);
        }

        foreach (var threadChannel in await guild.GetThreadChannelsAsync())
        {
            var channel = guildChannels.FirstOrDefault(o => o.IsThread() && o.ChannelId == threadChannel.Id.ToString() && o.ParentChannelId == threadChannel.CategoryId.ToString());
            if (channel == null) continue;

            channel.Name = threadChannel.Name;
            channel.MarkDeleted(threadChannel.IsArchived);
        }
    }
}
