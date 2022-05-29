using Discord.Commands;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Helpers;
using GrillBot.App.Infrastructure.Preconditions.TextBased;
using GrillBot.App.Modules.Implementations.Channels;
using GrillBot.Common.Extensions;
using GrillBot.Data.Extensions;
using GrillBot.Data.Extensions.Discord;
using GrillBot.Database.Enums;

namespace GrillBot.App.Modules.TextBased;

[Group("channel")]
[Name("Správa kanálů")]
[RequireUserPerms(ContextType.Guild)]
public class ChannelModule : Infrastructure.ModuleBase
{
    private GrillBotDatabaseFactory DbFactory { get; }

    public ChannelModule(GrillBotDatabaseFactory dbFactory)
    {
        DbFactory = dbFactory;
    }

    [Command("board")]
    [Summary("Získání TOP 10 kompletních statistik z kanálů, kam má uživatel přístup.")]
    public async Task GetChannelBoardAsync()
    {
        await Context.Guild.DownloadUsersAsync();
        if (Context.User is not SocketGuildUser user)
            user = Context.Guild.GetUser(Context.User.Id);

        var availableChannels = Context.Guild.GetAvailableTextChannelsFor(user).Select(o => o.Id.ToString()).ToList();

        using var dbContext = DbFactory.Create();

        var query = dbContext.UserChannels.AsNoTracking()
            .Where(o => o.GuildId == Context.Guild.Id.ToString() && o.Count > 0 && (o.Channel.Flags & (long)ChannelFlags.StatsHidden) == 0 && availableChannels.Contains(o.ChannelId));

        var groupedDataQuery = query.GroupBy(o => new { o.GuildId, o.ChannelId }).Select(o => new
        {
            o.Key.ChannelId,
            Count = o.Sum(x => x.Count)
        }).OrderByDescending(o => o.Count).Select(o => new KeyValuePair<string, long>(o.ChannelId, o.Count));

        if (!await groupedDataQuery.AnyAsync())
        {
            await ReplyAsync("Ještě nebyly zachyceny žádné události ukazující aktivitu serveru.");
            return;
        }

        var groupedData = await groupedDataQuery.Take(10).ToListAsync();

        var embed = new ChannelboardBuilder()
            .WithChannelboard(Context.User, Context.Guild, groupedData, id => Context.Guild.GetTextChannel(id), 0);

        var message = await ReplyAsync(embed: embed.Build());
        await message.AddReactionsAsync(Emojis.PaginationEmojis);
    }

    [Command]
    [Summary("Získání statistiky zpráv z jednotlivého kanálu.")]
    public async Task GetStatisticsOfChannelAsync(SocketTextChannel channel)
    {
        var isThread = channel is IThreadChannel;

        await Context.Guild.DownloadUsersAsync();
        if (!channel.HaveAccess(Context.User is SocketGuildUser sgu ? sgu : Context.Guild.GetUser(Context.User.Id)))
        {
            await ReplyAsync($"Promiň, ale do tohoto {(isThread ? "vlákna" : "kanálu")} nemáš přístup.");
            return;
        }

        using var dbContext = DbFactory.Create();

        var channelDataQuery = dbContext.UserChannels.AsNoTracking()
            .Where(o => o.GuildId == Context.Guild.Id.ToString() && o.ChannelId == channel.Id.ToString() && o.Count > 0);

        var groupedDataQuery = channelDataQuery.GroupBy(o => new { o.GuildId, o.ChannelId }).Select(o => new
        {
            o.Key.ChannelId,
            Count = o.Sum(x => x.Count),
            FirstMessageAt = o.Min(x => x.FirstMessageAt),
            LastMessageAt = o.Max(x => x.LastMessageAt)
        });

        var channelData = await groupedDataQuery.FirstOrDefaultAsync();
        if (channelData == null)
        {
            await ReplyAsync($"Promiň, ale zatím nemám informace o aktivitě v tomto {(isThread ? "vláknu" : "kanálu")}.");
            return;
        }

        var topTenQuery = channelDataQuery.OrderByDescending(o => o.Count).ThenByDescending(o => o.LastMessageAt).Take(10);
        var topTenData = await topTenQuery.ToListAsync();
        var topTenFormatted = string.Join("\n", topTenData.Select((o, i) =>
        {
            var user = Context.Guild.GetUser(o.UserId.ToUlong());
            return $"**{i + 1,2}.** {(user == null ? "*(Neznámý uživatel)*" : user.GetDisplayName())} ({FormatHelper.FormatMessagesToCzech(o.Count)})";
        }));

        var embed = new EmbedBuilder()
            .WithFooter(Context.User)
            .WithAuthor($"Statistika aktivity {(isThread ? "ve vláknu" : "v kanálu")}.")
            .WithColor(Color.Blue)
            .WithCurrentTimestamp()
            .WithTitle((isThread ? "" : "#") + channel.Name)
            .AddField("Vytvořeno", channel.CreatedAt.LocalDateTime.ToCzechFormat(), true)
            .AddField("Počet zpráv", FormatHelper.FormatMessagesToCzech(channelData.Count), true)
            .AddField("První zpráva", channelData.FirstMessageAt == DateTime.MinValue ? "Není známo" : channelData.FirstMessageAt.ToCzechFormat(), true)
            .AddField("Poslední zpráva", channelData.LastMessageAt.ToCzechFormat(), true)
            .AddField("Počet uživatelů", FormatHelper.FormatMembersToCzech(channel.Users.Count), true);

        if (!isThread)
            embed.AddField("Počet oprávnění", FormatHelper.FormatPermissionstoCzech(channel.PermissionOverwrites.Count), true);

        embed.AddField("TOP 10 uživatelů", topTenFormatted, false);
        await ReplyAsync(embed: embed.Build());
    }
}
