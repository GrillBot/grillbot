using Discord.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Modules.Implementations.Channels;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Database.Enums;

namespace GrillBot.App.Modules.Interactions;

[Group("channel", "Informace o kanálech")]
[RequireUserPerms]
public class ChannelModule : InteractionsModuleBase
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public ChannelModule(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    [SlashCommand("info", "Informace o kanálu")]
    public async Task GetChannelInfoAsync(SocketGuildChannel channel)
    {
        var user = Context.User is IGuildUser guildUser ? guildUser : await Context.Client.TryFindGuildUserAsync(Context.Guild.Id, Context.User.Id);
        var haveAccess = await channel.HaveAccessAsync(user);

        if (!haveAccess)
        {
            await SetResponseAsync("Informace o kanálu ti nemohu dát, protože tam nemáš přístup.");
            return;
        }

        var channelType = channel.GetChannelType();
        var isThread = channelType == ChannelType.NewsThread || channelType == ChannelType.PrivateThread || channelType == ChannelType.PublicThread;
        var isCategory = channelType == ChannelType.Category;

        var channelEmbed = new EmbedBuilder()
            .WithFooter(user)
            .WithColor(Color.Blue)
            .WithCurrentTimestamp()
            .WithTitle((isThread ? "" : "#") + channel.Name)
            .AddField("Založeno", channel.CreatedAt.LocalDateTime.ToCzechFormat(), true);

        if (!isCategory)
            channelEmbed.AddField("Uživatelů", FormatHelper.FormatMembersToCzech(channel.Users.Count), true);

        if (channelType == ChannelType.News)
            channelEmbed.WithAuthor("Informace o kanálu s novinkami");
        else if (channelType == ChannelType.Voice)
            channelEmbed.WithAuthor("Informace o hlasovém kanálu");
        else if (isThread)
            channelEmbed.WithAuthor("Informace o vláknu");
        else if (channelType == ChannelType.Stage)
            channelEmbed.WithAuthor("Informace o jevišti");
        else if (channelType == ChannelType.Text)
            channelEmbed.WithAuthor("Informace o textovém kanálu");
        else if (isCategory)
            channelEmbed.WithAuthor("Informace o kategorii");
        else
            channelEmbed.WithAuthor($"Informace o neznámém typu kanálu ({channelType})");

        if (!isThread && !isCategory)
        {
            var permissionGroups = channel.PermissionOverwrites.GroupBy(o => o.TargetType).ToDictionary(o => o.Key, o => o.Count());
            var userPermsCount = permissionGroups.GetValueOrDefault(PermissionTarget.User);
            var rolePermsCount = permissionGroups.GetValueOrDefault(PermissionTarget.Role);
            var permsFormatted = $"Uživatelské: {FormatHelper.FormatPermissionstoCzech(userPermsCount)}\n" +
                                 $"Role: {FormatHelper.FormatPermissionstoCzech(rolePermsCount)}";

            channelEmbed.AddField("Počet oprávnění", permsFormatted);
        }

        if (isThread)
            channelEmbed.AddField("Kanál", (channel as SocketThreadChannel)!.ParentChannel!.GetMention(), true);

        await using var repository = DatabaseBuilder.CreateRepository();
        var channelData = await repository.Channel.FindChannelByIdAsync(channel.Id, channel.Guild.Id, true, true);

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
                .Where(o => !string.IsNullOrEmpty(o));

            if (flagsData.Any())
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

    [SlashCommand("board", "TOP 10 statistik kanálů, kam máš přístup.")]
    public async Task GetChannelboardAsync()
    {
        var user = Context.User is IGuildUser guildUser ? guildUser : await Context.Client.TryFindGuildUserAsync(Context.Guild.Id, Context.User.Id);
        var availableChannels = await Context.Guild.GetAvailableChannelsAsync(user, true);

        if (availableChannels.Count == 0)
        {
            await SetResponseAsync("Nemáš přístup do žádného kanálu.");
            return;
        }

        var availableChannelIds = availableChannels.ConvertAll(o => o.Id.ToString());

        using var repository = DatabaseBuilder.CreateRepository();
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
