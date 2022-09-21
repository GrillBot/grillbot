using AutoMapper;
using GrillBot.App.Services.AuditLog;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Data.Models;

namespace GrillBot.App.Services.Channels;

public class ChannelApiService
{
    private AutoReplyService AutoReplyService { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }
    private IMapper Mapper { get; }
    private ApiRequestContext ApiRequestContext { get; }
    private AuditLogWriter AuditLogWriter { get; }

    public ChannelApiService(GrillBotDatabaseBuilder databaseBuilder, IMapper mapper, IDiscordClient client, AutoReplyService autoReplyService, ApiRequestContext apiRequestContext,
        AuditLogWriter auditLogWriter)
    {
        AutoReplyService = autoReplyService;
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        DiscordClient = client;
        ApiRequestContext = apiRequestContext;
        AuditLogWriter = auditLogWriter;
    }

    public async Task<bool> UpdateChannelAsync(ulong id, UpdateChannelParams parameters)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var channel = await repository.Channel.FindChannelByIdAsync(id);
        if (channel == null)
            throw new NotFoundException();

        var before = new AuditChannelInfo(channel);
        var reloadAutoReply = channel.HasFlag(ChannelFlags.AutoReplyDeactivated) != ((parameters.Flags & (long)ChannelFlags.AutoReplyDeactivated) != 0);

        channel.Flags = parameters.Flags;
        var success = await repository.CommitAsync() > 0;

        if (!success)
            return false;

        if (reloadAutoReply)
            await AutoReplyService.InitAsync();

        var auditLogItem = new AuditLogDataWrapper(
            AuditLogItemType.ChannelUpdated,
            new Diff<AuditChannelInfo>(before, new AuditChannelInfo(channel)),
            processedUser: ApiRequestContext.LoggedUser
        );
        await AuditLogWriter.StoreAsync(auditLogItem);

        return true;
    }

    public async Task<List<ChannelboardItem>> GetChannelBoardAsync()
    {
        var loggedUserId = ApiRequestContext.GetUserId();
        var mutualGuilds = await DiscordClient.FindMutualGuildsAsync(loggedUserId);
        var result = new List<ChannelboardItem>();

        foreach (var guild in mutualGuilds)
            result.AddRange(await GetChannelBoardOfGuildAsync(loggedUserId, guild));

        return result
            .OrderByDescending(o => o.Count)
            .ThenByDescending(o => o.LastMessageAt)
            .ToList();
    }

    private async Task<List<ChannelboardItem>> GetChannelBoardOfGuildAsync(ulong loggedUserId, IGuild guild)
    {
        var guildUser = await guild.GetUserAsync(loggedUserId);

        var availableChannels = await guild.GetAvailableChannelsAsync(guildUser, true);
        if (availableChannels.Count == 0) return new List<ChannelboardItem>();
        var availableChannelIds = availableChannels.ConvertAll(o => o.Id.ToString());

        await using var repository = DatabaseBuilder.CreateRepository();

        var channelStats = await repository.Channel.GetAvailableStatsAsync(guild, availableChannelIds);
        if (channelStats.Count == 0) return new List<ChannelboardItem>();

        var channelStatIds = channelStats.ConvertAll(o => o.channelId);
        var channels = await repository.Channel.GetVisibleChannelsAsync(guild.Id, channelStatIds, true, true);

        var result = new List<ChannelboardItem>();
        foreach (var channel in channels)
        {
            var stats = channelStats.Find(o => o.channelId == channel.ChannelId);

            var channelboardItem = Mapper.Map<ChannelboardItem>(channel);
            channelboardItem.Count = stats.count;
            channelboardItem.LastMessageAt = stats.lastMessageAt;
            channelboardItem.FirstMessageAt = stats.firstMessageAt;

            result.Add(channelboardItem);
        }

        return result;
    }
}
