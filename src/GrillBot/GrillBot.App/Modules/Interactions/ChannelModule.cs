using Discord.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Modules.Implementations.Channels;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Data.Exceptions;
using GrillBot.Database.Enums;
using GrillBot.Database.Enums.Internal;

namespace GrillBot.App.Modules.Interactions;

[Group("channel", "Channel information")]
[RequireUserPerms]
public class ChannelModule : InteractionsModuleBase
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private FormatHelper FormatHelper { get; }

    public ChannelModule(GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts, FormatHelper formatHelper, IServiceProvider serviceProvider) : base(texts, serviceProvider)
    {
        DatabaseBuilder = databaseBuilder;
        FormatHelper = formatHelper;
    }

    [SlashCommand("info", "Channel information")]
    public async Task GetChannelInfoAsync(SocketGuildChannel channel)
    {
        var user = Context.User as IGuildUser ?? await Context.Client.TryFindGuildUserAsync(Context.Guild.Id, Context.User.Id);
        if (user == null)
            throw new InvalidOperationException(GetText(nameof(GetChannelInfoAsync), "UserNotFound"));

        var haveAccess = await channel.HaveAccessAsync(user);
        if (!haveAccess)
        {
            await SetResponseAsync(GetText(nameof(GetChannelInfoAsync), "NoAccess"));
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
            .AddField(GetText(nameof(GetChannelInfoAsync), "CreatedAt"), channel.CreatedAt.LocalDateTime.ToCzechFormat(), true);

        if (!isCategory && channelType != ChannelType.Forum)
        {
            channelEmbed.AddField(
                GetText(nameof(GetChannelInfoAsync), "MemberCount"),
                isThread ? FormatHelper.FormatMembersToCzech(((IThreadChannel)channel).MemberCount) : FormatHelper.FormatMembersToCzech(channel.Users.Count),
                true
            );
        }

        switch (channelType)
        {
            case ChannelType.News or ChannelType.Voice:
                channelEmbed.WithAuthor(GetText(nameof(GetChannelInfoAsync), $"{channelType}ChannelTitle"));
                break;
            default:
            {
                if (isThread)
                    channelEmbed.WithAuthor(GetText(nameof(GetChannelInfoAsync), "ThreadChannelTitle"));
                else
                    switch (channelType)
                    {
                        case ChannelType.Text or ChannelType.Stage or ChannelType.Forum:
                            channelEmbed.WithAuthor(GetText(nameof(GetChannelInfoAsync), $"{channelType}ChannelTitle"));
                            break;
                        default:
                        {
                            channelEmbed.WithAuthor(GetText(nameof(GetChannelInfoAsync), isCategory ? "CategoryChannelTitle" : "OtherChannelTitle".FormatWith(channelType.ToString())));
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
                var userPermsCountFormat = FormatHelper.FormatNumber(GetTextId(nameof(GetChannelInfoAsync), "PermsCountValue"), Locale, userPermsCount);
                var rolePermsCountFormat = FormatHelper.FormatNumber(GetTextId(nameof(GetChannelInfoAsync), "PermsCountValue"), Locale, rolePermsCount);
                var permsFormatted = GetText(nameof(GetChannelInfoAsync), "PermsCount").FormatWith(userPermsCountFormat, rolePermsCountFormat);

                channelEmbed.AddField(GetText(nameof(GetChannelInfoAsync), "PermsCountTitle"), permsFormatted);
                break;
            }
            case true:
                channelEmbed.AddField(GetText(nameof(GetChannelInfoAsync), "Channel"), (channel as SocketThreadChannel)!.ParentChannel!.GetMention(), true);
                break;
        }

        if (channelType == ChannelType.Forum)
        {
            if (!string.IsNullOrEmpty(forum!.Topic))
                channelEmbed.WithDescription(forum.Topic.Cut(EmbedBuilder.MaxDescriptionLength));

            channelEmbed.AddField(GetText(nameof(GetChannelInfoAsync), "TagsCount"),
                FormatHelper.FormatNumber(GetTextId(nameof(GetChannelInfoAsync), "TagsCountValue"), Locale, forum.Tags.Count), true);

            var activeThreads = (await forum.GetActiveThreadsAsync()).Where(o => o.CategoryId == forum.Id).ToList();
            var privateThreadsCount = activeThreads.Count(o => o.Type == ThreadType.PrivateThread);
            var publicThreadsCount = activeThreads.Count(o => o.Type == ThreadType.PublicThread);
            var threadsFormatBuilder = new StringBuilder();
            if (publicThreadsCount > 0)
                threadsFormatBuilder.AppendLine(FormatHelper.FormatNumber(GetTextId(nameof(GetChannelInfoAsync), "PublicThreadCountValue"), Locale, publicThreadsCount));
            if (privateThreadsCount > 0)
                threadsFormatBuilder.AppendLine(FormatHelper.FormatNumber(GetTextId(nameof(GetChannelInfoAsync), "PrivateThreadCountValue"), Locale, publicThreadsCount));
            if (threadsFormatBuilder.Length > 0)
                channelEmbed.AddField(GetText(nameof(GetChannelInfoAsync), "ThreadCount"), threadsFormatBuilder.ToString());
        }

        await using var repository = DatabaseBuilder.CreateRepository();
        var channelData = await repository.Channel.FindChannelByIdAsync(channel.Id, channel.Guild.Id, true, ChannelsIncludeUsersMode.IncludeExceptInactive);

        if (channelData != null)
        {
            var firstMessage = channelData.Users.Min(o => o.FirstMessageAt);
            var lastMessage = channelData.Users.Max(o => o.LastMessageAt);

            channelEmbed
                .AddField(GetText(nameof(GetChannelInfoAsync), "MessageCount"),
                    FormatHelper.FormatNumber(GetTextId(nameof(GetChannelInfoAsync), "MessageCountValue"), Locale, channelData.Users.Sum(o => o.Count)), true);

            if (firstMessage != DateTime.MinValue)
                channelEmbed.AddField(GetText(nameof(GetChannelInfoAsync), "FirstMessage"), firstMessage.ToCzechFormat(), true);
            if (lastMessage != DateTime.MinValue)
                channelEmbed.AddField(GetText(nameof(GetChannelInfoAsync), "LastMessage"), lastMessage.ToCzechFormat(), true);

            var flagsData = Enum.GetValues<ChannelFlags>()
                .Where(o => channelData.HasFlag(o))
                .Select(o => o switch
                {
                    ChannelFlags.CommandsDisabled => GetText(nameof(GetChannelInfoAsync), "Flags/CommandsDisabled"),
                    ChannelFlags.AutoReplyDeactivated => GetText(nameof(GetChannelInfoAsync), "Flags/AutoReplyDeactivated"),
                    ChannelFlags.StatsHidden => GetText(nameof(GetChannelInfoAsync), "Flags/StatsHidden"),
                    _ => null
                })
                .Where(o => !string.IsNullOrEmpty(o))
                .ToList();

            if (flagsData.Count > 0)
                channelEmbed.AddField(GetText(nameof(GetChannelInfoAsync), "Configuration"), string.Join("\n", flagsData));

            // Show statistics only if channel not have hidden stats or command was executed in the channel with hidden stats.
            if ((!channelData.HasFlag(ChannelFlags.StatsHidden) || channel.Id == Context.Channel.Id) && channelData.Users.Count > 0)
            {
                var topTenQuery = channelData.Users.OrderByDescending(o => o.Count).ThenByDescending(o => o.LastMessageAt).Take(10);
                var topTenData = topTenQuery.Select((o, i) => $"**{i + 1,2}.** {o.User!.FullName(true)} ({FormatHelper.FormatMessagesToCzech(o.Count)})");

                channelEmbed.AddField(GetText(nameof(GetChannelInfoAsync), "TopTen"), string.Join("\n", topTenData));
            }
        }

        await SetResponseAsync(embed: channelEmbed.Build());
    }

    [SlashCommand("board", "TOP 10 channel statistics you can access.")]
    public async Task GetChannelboardAsync()
    {
        using var command = GetCommand<Actions.Commands.GetChannelboard>();

        try
        {
            var (embed, paginationComponents) = await command.Command.ProcessAsync(0);
            await SetResponseAsync(embed: embed, components: paginationComponents);
        }
        catch (NotFoundException ex)
        {
            await SetResponseAsync(ex.Message);
        }
    }

    [RequireSameUserAsAuthor]
    [ComponentInteraction("channelboard:*", ignoreGroupNames: true)]
    public async Task HandleChannelboardPaginationAsync(int page)
    {
        var handler = new ChannelboardPaginationHandler(ServiceProvider, page);
        await handler.ProcessAsync(Context);
    }
}
