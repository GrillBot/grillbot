using Discord.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Modules.Implementations.Channels;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers;
using GrillBot.Database.Enums;
using GrillBot.Database.Enums.Internal;

namespace GrillBot.App.Modules.Interactions;

[Group("channel", "Channel information")]
[RequireUserPerms]
public class ChannelModule : InteractionsModuleBase
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private FormatHelper FormatHelper { get; }

    public ChannelModule(GrillBotDatabaseBuilder databaseBuilder, LocalizationManager localization,
        FormatHelper formatHelper) : base(localization)
    {
        DatabaseBuilder = databaseBuilder;
        FormatHelper = formatHelper;
    }

    [SlashCommand("info", "Channel information")]
    public async Task GetChannelInfoAsync(SocketGuildChannel channel)
    {
        var user = Context.User as IGuildUser ?? await Context.Client.TryFindGuildUserAsync(Context.Guild.Id, Context.User.Id);
        if (user == null)
            throw new InvalidOperationException(GetLocale(nameof(GetChannelInfoAsync), "UserNotFound"));

        var haveAccess = await channel.HaveAccessAsync(user);
        if (!haveAccess)
        {
            await SetResponseAsync(GetLocale(nameof(GetChannelInfoAsync), "NoAccess"));
            return;
        }

        var forum = channel as IForumChannel;
        var channelType = channel.GetChannelType();
        if (channelType == null && forum != null) channelType = ChannelType.Forum;
        var isThread = channelType is ChannelType.NewsThread or ChannelType.PrivateThread or ChannelType.PublicThread;
        var isCategory = channelType == ChannelType.Category;

        var channelEmbed = new EmbedBuilder()
            .WithFooter(user)
            .WithColor(Color.Blue)
            .WithCurrentTimestamp()
            .WithTitle((isThread ? "" : "#") + channel.Name)
            .AddField(GetLocale(nameof(GetChannelInfoAsync), "CreatedAt"), channel.CreatedAt.LocalDateTime.ToCzechFormat(), true);

        if (!isCategory && channelType != ChannelType.Forum)
        {
            channelEmbed.AddField(
                GetLocale(nameof(GetChannelInfoAsync), "MemberCount"),
                isThread ? FormatHelper.FormatMembersToCzech(((IThreadChannel)channel).MemberCount) : FormatHelper.FormatMembersToCzech(channel.Users.Count),
                true
            );
        }

        switch (channelType)
        {
            case ChannelType.News or ChannelType.Voice:
                channelEmbed.WithAuthor(GetLocale(nameof(GetChannelInfoAsync), $"{channelType}ChannelTitle"));
                break;
            default:
            {
                if (isThread)
                    channelEmbed.WithAuthor(GetLocale(nameof(GetChannelInfoAsync), "ThreadChannelTitle"));
                else
                    switch (channelType)
                    {
                        case ChannelType.Text or ChannelType.Stage or ChannelType.Forum:
                            channelEmbed.WithAuthor(GetLocale(nameof(GetChannelInfoAsync), $"{channelType}ChannelTitle"));
                            break;
                        default:
                        {
                            channelEmbed.WithAuthor(GetLocale(nameof(GetChannelInfoAsync), isCategory ? "CategoryChannelTitle" : "OtherChannelTitle".FormatWith(channelType.ToString())));
                            break;
                        }
                    }

                break;
            }
        }

        switch (isThread)
        {
            case false when !isCategory:
            {
                var permissionGroups = channel.PermissionOverwrites.GroupBy(o => o.TargetType).ToDictionary(o => o.Key, o => o.Count());
                var userPermsCount = permissionGroups.GetValueOrDefault(PermissionTarget.User);
                var rolePermsCount = permissionGroups.GetValueOrDefault(PermissionTarget.Role);
                var userPermsCountFormat = FormatHelper.FormatNumber(GetLocaleId(nameof(GetChannelInfoAsync), "PermsCountValue"), Locale, userPermsCount);
                var rolePermsCountFormat = FormatHelper.FormatNumber(GetLocaleId(nameof(GetChannelInfoAsync), "PermsCountValue"), Locale, rolePermsCount);
                var permsFormatted = GetLocale(nameof(GetChannelInfoAsync), "PermsCount").FormatWith(userPermsCountFormat, rolePermsCountFormat);

                channelEmbed.AddField(GetLocale(nameof(GetChannelInfoAsync), "PermsCountTitle"), permsFormatted);
                break;
            }
            case true:
                channelEmbed.AddField(GetLocale(nameof(GetChannelInfoAsync), "Channel"), (channel as SocketThreadChannel)!.ParentChannel!.GetMention(), true);
                break;
        }

        if (channelType == ChannelType.Forum)
        {
            if (!string.IsNullOrEmpty(forum!.Topic))
                channelEmbed.WithDescription(forum.Topic.Cut(EmbedBuilder.MaxDescriptionLength));

            channelEmbed.AddField(GetLocale(nameof(GetChannelInfoAsync), "TagsCount"),
                FormatHelper.FormatNumber(GetLocaleId(nameof(GetChannelInfoAsync), "TagsCountValue"), Locale, forum.Tags.Count), true);

            var activeThreads = (await forum.GetActiveThreadsAsync()).Where(o => o.CategoryId == forum.Id).ToList();
            var privateThreadsCount = activeThreads.Count(o => o.Type == ThreadType.PrivateThread);
            var publicThreadsCount = activeThreads.Count(o => o.Type == ThreadType.PublicThread);
            var threadsFormatBuilder = new StringBuilder();
            if (publicThreadsCount > 0)
                threadsFormatBuilder.AppendLine(FormatHelper.FormatNumber(GetLocaleId(nameof(GetChannelInfoAsync), "PublicThreadCountValue"), Locale, publicThreadsCount));
            if (privateThreadsCount > 0)
                threadsFormatBuilder.AppendLine(FormatHelper.FormatNumber(GetLocaleId(nameof(GetChannelInfoAsync), "PrivateThreadCountValue"), Locale, publicThreadsCount));
            if (threadsFormatBuilder.Length > 0)
                channelEmbed.AddField(GetLocale(nameof(GetChannelInfoAsync), "ThreadCount"), threadsFormatBuilder.ToString());
        }

        await using var repository = DatabaseBuilder.CreateRepository();
        var channelData = await repository.Channel.FindChannelByIdAsync(channel.Id, channel.Guild.Id, true, ChannelsIncludeUsersMode.IncludeExceptInactive);

        if (channelData != null)
        {
            var firstMessage = channelData.Users.Min(o => o.FirstMessageAt);
            var lastMessage = channelData.Users.Max(o => o.LastMessageAt);

            channelEmbed
                .AddField(GetLocale(nameof(GetChannelInfoAsync), "MessageCount"),
                    FormatHelper.FormatNumber(GetLocaleId(nameof(GetChannelInfoAsync), "MessageCountValue"), Locale, channelData.Users.Sum(o => o.Count)), true);

            if (firstMessage != DateTime.MinValue)
                channelEmbed.AddField(GetLocale(nameof(GetChannelInfoAsync), "FirstMessage"), firstMessage.ToCzechFormat(), true);
            if (lastMessage != DateTime.MinValue)
                channelEmbed.AddField(GetLocale(nameof(GetChannelInfoAsync), "LastMessage"), lastMessage.ToCzechFormat(), true);

            var flagsData = Enum.GetValues<ChannelFlags>()
                .Where(o => channelData.HasFlag(o))
                .Select(o => o switch
                {
                    ChannelFlags.CommandsDisabled => GetLocale(nameof(GetChannelInfoAsync), "Flags/CommandsDisabled"),
                    ChannelFlags.AutoReplyDeactivated => GetLocale(nameof(GetChannelInfoAsync), "Flags/AutoReplyDeactivated"),
                    ChannelFlags.StatsHidden => GetLocale(nameof(GetChannelInfoAsync), "Flags/StatsHidden"),
                    _ => null
                })
                .Where(o => !string.IsNullOrEmpty(o))
                .ToList();

            if (flagsData.Count > 0)
                channelEmbed.AddField(GetLocale(nameof(GetChannelInfoAsync), "Configuration"), string.Join("\n", flagsData));

            if (!channelData.HasFlag(ChannelFlags.StatsHidden))
            {
                var topTenQuery = channelData.Users.OrderByDescending(o => o.Count).ThenByDescending(o => o.LastMessageAt).Take(10);
                var topTenData = topTenQuery.Select((o, i) => $"**{i + 1,2}.** {Context.Guild.GetUser(o.UserId.ToUlong())?.GetDisplayName()} ({FormatHelper.FormatMessagesToCzech(o.Count)})");

                channelEmbed.AddField(GetLocale(nameof(GetChannelInfoAsync), "TopTen"), string.Join("\n", topTenData));
            }
        }

        await SetResponseAsync(embed: channelEmbed.Build());
    }

    [SlashCommand("board", "TOP 10 channel statistics you can access.")]
    public async Task GetChannelboardAsync()
    {
        var user = Context.User as IGuildUser ?? await Context.Client.TryFindGuildUserAsync(Context.Guild.Id, Context.User.Id);
        if (user == null)
            throw new InvalidOperationException(GetLocale(nameof(GetChannelboardAsync), "UserNotFound"));

        var availableChannels = await Context.Guild.GetAvailableChannelsAsync(user, true);
        if (availableChannels.Count == 0)
        {
            await SetResponseAsync(GetLocale(nameof(GetChannelboardAsync), "NoAccess"));
            return;
        }

        var availableChannelIds = availableChannels.ConvertAll(o => o.Id.ToString());

        await using var repository = DatabaseBuilder.CreateRepository();
        var channels = await repository.Channel.GetVisibleChannelsAsync(Context.Guild.Id, availableChannelIds, true);

        if (channels.Count == 0)
        {
            await SetResponseAsync(GetLocale(nameof(GetChannelboardAsync), "NoActivity"));
            return;
        }

        var data = channels
            .Select(o => new { o.ChannelId, Count = o.Users.Sum(x => x.Count) })
            .OrderByDescending(o => o.Count)
            .Take(10)
            .ToDictionary(o => o.ChannelId, o => o.Count);

        var embed = new EmbedBuilder()
            .WithChannelboard(user, Context.Guild, data, id => Context.Guild.GetTextChannel(id), 0);

        var pagesCount = (int)Math.Ceiling(channels.Count / 10.0D);
        var components = ComponentsHelper.CreatePaginationComponents(0, pagesCount, "channelboard");
        await SetResponseAsync(embed: embed.Build(), components: components);
    }

    [RequireSameUserAsAuthor]
    [ComponentInteraction("channelboard:*", ignoreGroupNames: true)]
    public async Task HandleChannelboardPaginationAsync(int page)
    {
        var handler = new ChannelboardPaginationHandler(Context.Client, DatabaseBuilder, page);
        await handler.ProcessAsync(Context);
    }
}
