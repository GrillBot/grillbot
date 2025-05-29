using GrillBot.App.Infrastructure.Embeds;
using GrillBot.App.Modules.Implementations.Channels;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Extensions;

namespace GrillBot.App.Actions.Commands;

public class GetChannelboard(
    GrillBotDatabaseBuilder _databaseBuilder,
    ITextsManager _texts,
    FormatHelper _formatHelper
) : CommandAction
{
    private const int ItemsOnPage = 10;

    public async Task<(Embed embed, MessageComponent? paginationComponents)> ProcessAsync(int page)
    {
        var visibleChannels = await GetAvailableChannelsAsync();
        var visibleChannelIds = visibleChannels.Select(o => o.Id.ToString()).ToHashSet();

        using var repository = _databaseBuilder.CreateRepository();
        var statistics = await repository.Channel.GetAvailableStatsAsync(Context.Guild, visibleChannelIds);

        if (statistics.Count == 0)
            return (CreateEmbed(_texts["ChannelModule/GetChannelboard/NoActivity", Locale], page), null);

        var skip = page * ItemsOnPage;
        var result = statistics
            .OrderByDescending(o => o.Value.count)
            .Skip(skip).Take(ItemsOnPage)
            .ToDictionary(o => o.Key, o => o.Value);
        var description = await CreateDescriptionAsync(result, skip);
        var embed = CreateEmbed(description, page);
        var pagesCount = ComputePagesCount(statistics.Count);
        var paginationComponents = ComponentsHelper.CreatePaginationComponents(page, pagesCount, "channelboard");
        return (embed, paginationComponents);
    }

    public async Task<int> ComputePagesCountAsync()
    {
        var visibleChannels = await GetAvailableChannelsAsync();

        using var repository = _databaseBuilder.CreateRepository();
        var statisticsCount = await repository.Channel.GetAvailableStatsCountAsync(Context.Guild, visibleChannels.Select(o => o.Id.ToString()));

        return ComputePagesCount(statisticsCount);
    }

    private static int ComputePagesCount(int statisticsCount)
        => (int)Math.Ceiling(statisticsCount / (double)ItemsOnPage);

    private async Task<List<IGuildChannel>> GetAvailableChannelsAsync()
    {
        var availableChannels = await Context.Guild.GetAvailableChannelsAsync((IGuildUser)Context.User, true, false);
        if (availableChannels.Count == 0)
            throw new NotFoundException(_texts["ChannelModule/GetChannelboard/NoAccess", Locale]);

        return availableChannels;
    }

    private Embed CreateEmbed(string description, int page)
    {
        return new EmbedBuilder()
            .WithFooter(Context.User)
            .WithMetadata(new ChannelboardMetadata { Page = page })
            .WithAuthor(_texts["ChannelModule/GetChannelboard/Title", Locale])
            .WithColor(Color.Blue)
            .WithCurrentTimestamp()
            .WithDescription(description)
            .Build();
    }

    private async Task<string> CreateDescriptionAsync(Dictionary<string, (long count, DateTime firstMessageAt, DateTime lastMessageAt)> statistics, int skip)
    {
        var result = new List<string>();
        var template = _texts["ChannelModule/GetChannelboard/Row", Locale];
        for (var i = 0; i < statistics.Count; i++)
        {
            var statItem = statistics.ElementAt(i);
            var order = (i + skip + 1).ToString().PadLeft(2, '0');
            var channel = await Context.Guild.GetChannelAsync(statItem.Key.ToUlong());
            var count = _formatHelper.FormatNumber("ChannelModule/GetChannelboard/Counts", Locale, statItem.Value.count);

            result.Add(template.FormatWith(order, channel.Name, count));
        }

        return string.Join("\n", result);
    }
}
