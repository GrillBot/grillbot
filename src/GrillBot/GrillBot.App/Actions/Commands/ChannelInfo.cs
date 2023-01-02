using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Database.Enums;
using GrillBot.Database.Enums.Internal;

namespace GrillBot.App.Actions.Commands;

public class ChannelInfo : CommandAction
{
    private ITextsManager Texts { get; }
    private FormatHelper FormatHelper { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public string ErrorMessage { get; private set; }
    public bool IsOk => string.IsNullOrEmpty(ErrorMessage);

    public ChannelInfo(ITextsManager texts, FormatHelper formatHelper, GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
        FormatHelper = formatHelper;
        Texts = texts;
    }

    public async Task<Embed> ProcessAsync(IGuildChannel channel)
    {
        await CheckAccessAsync(channel);
        if (!IsOk) return null;

        var channelType = GetChannelType(channel);
        var isThread = channelType is ChannelType.NewsThread or ChannelType.PrivateThread or ChannelType.PublicThread;
        var builder = Init(channel, isThread);
        SetTitle(builder, channelType);

        if (channelType != ChannelType.Category && channelType != ChannelType.Forum) await SetMemberCountAsync(builder, channel, isThread);
        switch (isThread)
        {
            case true:
                await SetThreadInfoAsync(builder, channel);
                break;
            case false when channelType != ChannelType.Category:
                SetPermissionsInfo(builder, channel);
                break;
        }

        if (channelType == ChannelType.Forum) await SetForumInfoAsync(builder, channel);
        await SetStatisticsAndConfigurationAsync(builder, channel);

        return builder.Build();
    }

    private async Task CheckAccessAsync(IGuildChannel channel)
    {
        if (await channel.HaveAccessAsync((IGuildUser)Context.User)) return;
        ErrorMessage = Texts["ChannelModule/ChannelInfo/NoAccess", Locale];
    }

    private static ChannelType GetChannelType(IChannel channel)
        => channel is IForumChannel ? ChannelType.Forum : channel.GetChannelType()!.Value;

    private EmbedBuilder Init(IChannel channel, bool isThread)
    {
        return new EmbedBuilder()
            .WithFooter(Context.User)
            .WithColor(Color.Blue)
            .WithCurrentTimestamp()
            .WithTitle((isThread ? "" : "#") + channel.Name)
            .AddField(Texts["ChannelModule/ChannelInfo/CreatedAt", Locale], channel.CreatedAt.ToCzechFormat(), true);
    }

    private async Task SetMemberCountAsync(EmbedBuilder builder, IGuildChannel channel, bool isThread)
    {
        var memberCountValue = isThread ? ((IThreadChannel)channel).MemberCount : (await channel.GetUsersAsync().FlattenAsync()).Count();
        var memberCount = FormatHelper.FormatNumber("ChannelModule/ChannelInfo/MemberCountValue", Locale, memberCountValue);

        builder.AddField(Texts["ChannelModule/ChannelInfo/MemberCount", Locale], memberCount, true);
    }

    private void SetTitle(EmbedBuilder builder, ChannelType channelType)
    {
        var textId = channelType switch
        {
            ChannelType.NewsThread or ChannelType.PrivateThread or ChannelType.PublicThread => "ThreadChannelTitle",
            ChannelType.News or ChannelType.Voice or ChannelType.Text or ChannelType.Stage or ChannelType.Forum or ChannelType.Category => $"{channelType}ChannelTitle",
            _ => "OtherChannelType"
        };

        var title = Texts[$"ChannelModule/ChannelInfo/{textId}", Locale];
        if (textId == "OtherChannelType") title = title.FormatWith(channelType.ToString());
        builder.WithAuthor(title);
    }

    private async Task SetThreadInfoAsync(EmbedBuilder builder, IGuildChannel channel)
    {
        var parentChannelId = ((IThreadChannel)channel).CategoryId!.Value!;
        var parentChannel = await Context.Guild.GetChannelAsync(parentChannelId);

        if (parentChannel != null)
            builder.AddField(Texts["ChannelModule/ChannelInfo/Channel", Locale], parentChannel.GetMention(), true);
    }

    private void SetPermissionsInfo(EmbedBuilder builder, IGuildChannel channel)
    {
        var permissionGroups = channel.PermissionOverwrites.GroupBy(o => o.TargetType).ToDictionary(o => o.Key, o => o.Count());
        var userPermsCount = permissionGroups.GetValueOrDefault(PermissionTarget.User);
        var rolePermsCount = permissionGroups.GetValueOrDefault(PermissionTarget.Role);
        var userPermsCountFormat = FormatHelper.FormatNumber("ChannelModule/ChannelInfo/PermsCountValue", Locale, userPermsCount);
        var rolePermsCountFormat = FormatHelper.FormatNumber("ChannelModule/ChannelInfo/PermsCountValue", Locale, rolePermsCount);
        var permsFormatted = Texts["ChannelModule/ChannelInfo/PermsCount", Locale].FormatWith(userPermsCountFormat, rolePermsCountFormat);
        builder.AddField(Texts["ChannelModule/ChannelInfo/PermsCountTitle", Locale], permsFormatted);
    }

    private async Task SetForumInfoAsync(EmbedBuilder builder, IGuildChannel channel)
    {
        var forum = (IForumChannel)channel;

        if (!string.IsNullOrEmpty(forum.Topic))
            builder.WithDescription(forum.Topic.Cut(EmbedBuilder.MaxDescriptionLength));

        var tagsCount = FormatHelper.FormatNumber("ChannelModule/ChannelInfo/TagsCountValue", Locale, forum.Tags.Count);
        builder.AddField(Texts["ChannelModule/ChannelInfo/TagsCount", Locale], tagsCount, true);

        var activeThreads = (await forum.GetActiveThreadsAsync()).Where(o => o.CategoryId == forum.Id).ToList();
        var privateThreadsCount = activeThreads.Count(o => o.Type == ThreadType.PrivateThread);
        var publicThreadsCount = activeThreads.Count(o => o.Type == ThreadType.PublicThread);
        var threadsFormatBuilder = new StringBuilder();
        if (publicThreadsCount > 0)
            threadsFormatBuilder.AppendLine(FormatHelper.FormatNumber("ChannelModule/ChannelInfo/PublicThreadCountValue", Locale, publicThreadsCount));
        if (privateThreadsCount > 0)
            threadsFormatBuilder.AppendLine(FormatHelper.FormatNumber("ChannelModule/ChannelInfo/PrivateThreadCountValue", Locale, publicThreadsCount));
        if (threadsFormatBuilder.Length > 0)
            builder.AddField(Texts["ChannelModule/ChannelInfo/ThreadCount", Locale], threadsFormatBuilder.ToString());
    }

    private async Task SetStatisticsAndConfigurationAsync(EmbedBuilder builder, IGuildChannel channel)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var data = await repository.Channel.FindChannelByIdAsync(channel.Id, channel.GuildId, true, ChannelsIncludeUsersMode.IncludeExceptInactive);
        if (data == null) return;

        var messagesCount = FormatHelper.FormatNumber("ChannelModule/ChannelInfo/MessageCountValue", Locale, data.Users.Sum(o => o.Count));
        builder.AddField(Texts["ChannelModule/ChannelInfo/MessageCount", Locale], messagesCount, true);

        var firstMessage = data.Users.Count == 0 ? DateTime.MinValue : data.Users.Min(o => o.FirstMessageAt);
        var lastMessage = data.Users.Count == 0 ? DateTime.MinValue : data.Users.Max(o => o.LastMessageAt);
        if (firstMessage != DateTime.MinValue)
            builder.AddField(Texts["ChannelModule/ChannelInfo/FirstMessage", Locale], firstMessage.ToCzechFormat(), true);
        if (lastMessage != DateTime.MinValue)
            builder.AddField(Texts["ChannelModule/ChannelInfo/LastMessage", Locale], lastMessage.ToCzechFormat(), true);

        var flagsData = Enum.GetValues<ChannelFlag>()
            .Where(o => data.HasFlag(o))
            .Select(o => o switch
            {
                ChannelFlag.CommandsDisabled => Texts["ChannelModule/ChannelInfo/Flags/CommandsDisabled", Locale],
                ChannelFlag.AutoReplyDeactivated => Texts["ChannelModule/ChannelInfo/Flags/AutoReplyDeactivated", Locale],
                ChannelFlag.StatsHidden => Texts["ChannelModule/ChannelInfo/Flags/StatsHidden", Locale],
                _ => null
            })
            .Where(o => !string.IsNullOrEmpty(o))
            .ToList();
        if (flagsData.Count > 0)
            builder.AddField(Texts["ChannelModule/ChannelInfo/Configuration", Locale], string.Join("\n", flagsData));

        // Show statistics only if channel not have hidden stats or command was executed in the channel with hidden stats.
        if ((!data.HasFlag(ChannelFlag.StatsHidden) || channel.Id == Context.Channel.Id) && data.Users.Count > 0)
        {
            var topTenQuery = data.Users.OrderByDescending(o => o.Count).ThenByDescending(o => o.LastMessageAt).Take(10);
            var topTenData = topTenQuery.Select((o, i) =>
            {
                var messageCount = FormatHelper.FormatNumber("ChannelModule/ChannelInfo/MessageCountValue", Locale, o.Count);
                return $"**{i + 1,2}.** {o.User!.FullName(true)} ({messageCount})";
            });

            builder.AddField(Texts["ChannelModule/ChannelInfo/TopTen", Locale], string.Join("\n", topTenData));
        }
    }
}
