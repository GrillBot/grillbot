using Discord.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Modules.Implementations.Channels;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Database.Enums;
using GrillBot.Database.Enums.Internal;

namespace GrillBot.App.Modules.Interactions;

[Group("channel", "Channel information")]
[RequireUserPerms]
public class ChannelModule : InteractionsModuleBase
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public ChannelModule(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    [SlashCommand("info", "Channel information")]
    public async Task GetChannelInfoAsync(SocketGuildChannel channel)
    {
        var user = Context.User as IGuildUser ?? await Context.Client.TryFindGuildUserAsync(Context.Guild.Id, Context.User.Id);
        if (user == null)
            throw new InvalidOperationException("Nepodařilo se dohledat uživatele, která zavolal příkaz.");

        var haveAccess = await channel.HaveAccessAsync(user);
        if (!haveAccess)
        {
            await SetResponseAsync("Informace o kanálu ti nemohu dát, protože tam nemáš přístup.");
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
            .AddField("Založeno", channel.CreatedAt.LocalDateTime.ToCzechFormat(), true);

        if (!isCategory && channelType != ChannelType.Forum)
        {
            channelEmbed.AddField(
                "Počet členů",
                isThread ? FormatHelper.FormatMembersToCzech(((IThreadChannel)channel).MemberCount) : FormatHelper.FormatMembersToCzech(channel.Users.Count),
                true
            );
        }

        switch (channelType)
        {
            case ChannelType.News:
                channelEmbed.WithAuthor("Informace o kanálu s novinkami");
                break;
            case ChannelType.Voice:
                channelEmbed.WithAuthor("Informace o hlasovém kanálu");
                break;
            default:
            {
                if (isThread)
                    channelEmbed.WithAuthor("Informace o vláknu");
                else
                    switch (channelType)
                    {
                        case ChannelType.Stage:
                            channelEmbed.WithAuthor("Informace o jevišti");
                            break;
                        case ChannelType.Text:
                            channelEmbed.WithAuthor("Informace o textovém kanálu");
                            break;
                        case ChannelType.Forum:
                            channelEmbed.WithAuthor("Informace o fóru");
                            break;
                        default:
                        {
                            channelEmbed.WithAuthor(isCategory ? "Informace o kategorii" : $"Informace o neznámém typu kanálu ({channelType})");
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
                var permsFormatted = $"Uživatelské: {FormatHelper.FormatPermissionstoCzech(userPermsCount)}\n" +
                                     $"Role: {FormatHelper.FormatPermissionstoCzech(rolePermsCount)}";

                channelEmbed.AddField("Počet oprávnění", permsFormatted);
                break;
            }
            case true:
                channelEmbed.AddField("Kanál", (channel as SocketThreadChannel)!.ParentChannel!.GetMention(), true);
                break;
        }

        if (channelType == ChannelType.Forum)
        {
            if (!string.IsNullOrEmpty(forum!.Topic))
                channelEmbed.WithDescription(forum.Topic.Cut(EmbedBuilder.MaxDescriptionLength));

            channelEmbed.AddField("Počet tagů", FormatHelper.Format(forum.Tags.Count, "tag", "tagy", "tagů"), true);

            var activeThreads = (await forum.GetActiveThreadsAsync()).Where(o => o.CategoryId == forum.Id).ToList();
            var privateThreadsCount = activeThreads.Count(o => o.Type == ThreadType.PrivateThread);
            var publicThreadsCount = activeThreads.Count(o => o.Type == ThreadType.PublicThread);
            var threadsFormatBuilder = new StringBuilder();
            if (publicThreadsCount > 0)
                threadsFormatBuilder.AppendLine(FormatHelper.Format(publicThreadsCount, "veřejné vlákno", "veřejné vlákna", "veřejných vláken"));
            if (privateThreadsCount > 0)
                threadsFormatBuilder.AppendLine(FormatHelper.Format(privateThreadsCount, "soukromé vlákno", "soukromé vlákna", "soukromých vláken"));
            if (threadsFormatBuilder.Length > 0)
                channelEmbed.AddField("Počet vláken", threadsFormatBuilder.ToString());
        }

        await using var repository = DatabaseBuilder.CreateRepository();
        var channelData = await repository.Channel.FindChannelByIdAsync(channel.Id, channel.Guild.Id, true, ChannelsIncludeUsersMode.IncludeExceptInactive);

        if (channelData != null)
        {
            var firstMessage = channelData.Users.Min(o => o.FirstMessageAt);
            var lastMessage = channelData.Users.Max(o => o.LastMessageAt);

            channelEmbed
                .AddField("Počet zpráv", FormatHelper.FormatMessagesToCzech(channelData.Users.Sum(o => o.Count)), true);

            if (firstMessage != DateTime.MinValue)
                channelEmbed.AddField("První zpráva", firstMessage.ToCzechFormat(), true);
            if (lastMessage != DateTime.MinValue)
                channelEmbed.AddField("Poslední zpráva", lastMessage.ToCzechFormat(), true);

            var flagsData = Enum.GetValues<ChannelFlags>()
                .Where(o => channelData.HasFlag(o))
                .Select(o => o switch
                {
                    ChannelFlags.CommandsDisabled => "Deaktivovány všechny příkazy",
                    ChannelFlags.AutoReplyDeactivated => "Deaktivovány automatické odpovědi",
                    ChannelFlags.StatsHidden => "Skryté statistiky",
                    _ => null
                })
                .Where(o => !string.IsNullOrEmpty(o))
                .ToList();

            if (flagsData.Count > 0)
                channelEmbed.AddField("Konfigurace", string.Join("\n", flagsData));

            if (!channelData.HasFlag(ChannelFlags.StatsHidden))
            {
                var topTenQuery = channelData.Users.OrderByDescending(o => o.Count).ThenByDescending(o => o.LastMessageAt).Take(10);
                var topTenData = topTenQuery.Select((o, i) => $"**{i + 1,2}.** {Context.Guild.GetUser(o.UserId.ToUlong())?.GetDisplayName()} ({FormatHelper.FormatMessagesToCzech(o.Count)})");

                channelEmbed.AddField("TOP 10 uživatelů", string.Join("\n", topTenData));
            }
        }

        await SetResponseAsync(embed: channelEmbed.Build());
    }

    [SlashCommand("board", "TOP 10 channel statistics you can access.")]
    public async Task GetChannelboardAsync()
    {
        var user = Context.User as IGuildUser ?? await Context.Client.TryFindGuildUserAsync(Context.Guild.Id, Context.User.Id);
        if (user == null)
            throw new InvalidOperationException("Nepodařilo se dohledat uživatele, který zavolal příkaz.");

        var availableChannels = await Context.Guild.GetAvailableChannelsAsync(user, true);
        if (availableChannels.Count == 0)
        {
            await SetResponseAsync("Nemáš přístup do žádného kanálu.");
            return;
        }

        var availableChannelIds = availableChannels.ConvertAll(o => o.Id.ToString());

        await using var repository = DatabaseBuilder.CreateRepository();
        var channels = await repository.Channel.GetVisibleChannelsAsync(Context.Guild.Id, availableChannelIds, true);

        if (channels.Count == 0)
        {
            await SetResponseAsync("Doteď nebyla zaznamenána žádná aktivita v kanálech.");
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
