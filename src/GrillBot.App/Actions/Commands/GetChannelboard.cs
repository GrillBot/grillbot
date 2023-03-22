using GrillBot.App.Infrastructure.Embeds;
using GrillBot.App.Modules.Implementations.Channels;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Extensions;

namespace GrillBot.App.Actions.Commands;

public class GetChannelboard : CommandAction
{
    private const int ItemsOnPage = 10;

    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }
    private FormatHelper FormatHelper { get; }

    public GetChannelboard(GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts, FormatHelper formatHelper)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
        FormatHelper = formatHelper;
    }

    public async Task<(Embed embed, MessageComponent paginationComponents)> ProcessAsync(int page)
    {
        var visibleChannels = await GetAvailableChannelsAsync();

        await using var repository = DatabaseBuilder.CreateRepository();
        var statistics = await repository.Channel.GetAvailableStatsAsync(Context.Guild, visibleChannels.Select(o => o.Id.ToString()));

        if (statistics.Count == 0)
            return (CreateEmbed(Texts["ChannelModule/GetChannelboard/NoActivity", Locale], page), null);

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

        await using var repository = DatabaseBuilder.CreateRepository();
        var statistics = await repository.Channel.GetAvailableStatsAsync(Context.Guild, visibleChannels.Select(o => o.Id.ToString()));

        return ComputePagesCount(statistics.Count);
    }

    private static int ComputePagesCount(int statisticsCount)
        => (int)Math.Ceiling(statisticsCount / (double)ItemsOnPage);

    private async Task<List<IGuildChannel>> GetAvailableChannelsAsync()
    {
        var availableChannels = await Context.Guild.GetAvailableChannelsAsync((IGuildUser)Context.User, true, false);
        if (availableChannels.Count == 0)
            throw new NotFoundException(Texts["ChannelModule/GetChannelboard/NoAccess", Locale]);

        return availableChannels;
    }

    private Embed CreateEmbed(string description, int page)
    {
        return new EmbedBuilder()
            .WithFooter(Context.User)
            .WithMetadata(new ChannelboardMetadata { Page = page })
            .WithAuthor(Texts["ChannelModule/GetChannelboard/Title", Locale])
            .WithColor(Color.Blue)
            .WithCurrentTimestamp()
            .WithDescription(description)
            .Build();
    }

    private async Task<string> CreateDescriptionAsync(Dictionary<string, (long count, DateTime firstMessageAt, DateTime lastMessageAt)> statistics, int skip)
    {
        var result = new List<string>();
        var template = Texts["ChannelModule/GetChannelboard/Row", Locale];
        for (var i = 0; i < statistics.Count; i++)
        {
            var statItem = statistics.ElementAt(i);
            var order = (i + skip + 1).ToString().PadLeft(2, '0');
            var channel = await Context.Guild.GetChannelAsync(statItem.Key.ToUlong());
            var count = FormatHelper.FormatNumber("ChannelModule/GetChannelboard/Counts", Locale, statItem.Value.count);

            result.Add(template.FormatWith(order, channel.Name, count));
        }

        return string.Join("\n", result);
    }
}
