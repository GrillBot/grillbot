using GrillBot.App.Helpers;
using GrillBot.App.Infrastructure;
using GrillBot.Data.Extensions;
using GrillBot.Data.Models;
using GrillBot.Database.Entity;

namespace GrillBot.App.Services.Emotes;

public partial class EmoteService : ServiceBase
{
    private string CommandPrefix { get; }
    public ConcurrentBag<GuildEmote> SupportedEmotes { get; }
    private readonly object SupportedEmotesLock = new object();
    private MessageCache.MessageCache MessageCache { get; }

    public EmoteService(DiscordSocketClient client, GrillBotContextFactory dbFactory, IConfiguration configuration,
        MessageCache.MessageCache messageCache) : base(client, dbFactory)
    {
        CommandPrefix = configuration.GetValue<string>("Discord:Commands:Prefix");
        SupportedEmotes = new ConcurrentBag<GuildEmote>();
        MessageCache = messageCache;

        DiscordClient.Ready += OnReadyAsync;
        DiscordClient.MessageReceived += OnMessageReceivedAsync;
        DiscordClient.GuildAvailable += OnGuildAvailableAsync;
        DiscordClient.GuildUpdated += OnGuildUpdatedAsync;
        DiscordClient.MessageDeleted += OnMessageRemovedAsync;
        DiscordClient.ReactionAdded += (msg, channel, reaction) => OnReactionAsync(msg, channel, reaction, ReactionEvents.Added);
        DiscordClient.ReactionRemoved += (msg, channel, reaction) => OnReactionAsync(msg, channel, reaction, ReactionEvents.Removed);
    }

    public async Task<Embed> GetEmoteInfoAsync(IEmote emoteItem, IUser caller)
    {
        EnsureEmote(emoteItem, out var emote);

        using var context = DbFactory.Create();
        var baseQuery = context.Emotes.AsQueryable().Where(o => o.EmoteId == emote.ToString() && o.UseCount > 0);

        var queryData = baseQuery.GroupBy(o => o.EmoteId).Select(o => new
        {
            UsersCount = o.Count(),
            FirstOccurence = o.Min(x => x.FirstOccurence),
            LastOccurence = o.Max(x => x.LastOccurence),
            UseCount = o.Sum(x => x.UseCount),
            GuildId = o.Min(o => o.GuildId)
        });

        var data = await queryData.FirstOrDefaultAsync();
        if (data == null)
            return null;

        var guild = DiscordClient.GetGuild(Convert.ToUInt64(data.GuildId));
        var topTenQuery = baseQuery.OrderByDescending(x => x.UseCount).ThenByDescending(x => x.LastOccurence).Take(10);

        var topTen = await topTenQuery.AsAsyncEnumerable().SelectAwait(async (o, i) =>
        {
            var user = await DiscordClient.FindUserAsync(Convert.ToUInt64(o.UserId));
            return $"**{i + 1,2}.** {user?.GetDisplayName() ?? "Neznámý uživatel"} ({o.UseCount})";
        }).ToListAsync();

        var embed = new EmbedBuilder()
            .WithFooter(caller)
            .WithAuthor("Informace o emote")
            .WithColor(Color.Blue)
            .WithCurrentTimestamp()
            .AddField("Název", emote.Name, true)
            .AddField("Animován", FormatHelper.FormatBooleanToCzech(emote.Animated), true)
            .AddField("První výskyt", data.FirstOccurence.ToCzechFormat(), true)
            .AddField("Poslední výskyt", data.LastOccurence.ToCzechFormat(), true)
            .AddField("Od posl. použití", (DateTime.Now - data.LastOccurence).Humanize(culture: new CultureInfo("cs-CZ")), true)
            .AddField("Počet použití", data.UseCount, true)
            .AddField("Počet uživatelů", data.UsersCount, true)
            .AddField("Server", guild?.Name ?? "Neznámý server", true)
            .AddField("TOP 10 použití", string.Join("\n", topTen), false)
            .AddField("Odkaz", emote.Url, false)
            .WithThumbnailUrl(emote.Url);

        return embed.Build();
    }

    public void EnsureEmote(IEmote emote, out Emote result)
    {
        if (emote is not Emote _result)
            throw new ArgumentException("Unicode emoji nejsou v tomto příkazu podporovány.");

        result = _result;
    }

    private async Task<List<EmoteStatItem>> GetEmoteListAsync(IGuild guild, IUser ofUser,
        Func<IQueryable<IGrouping<string, EmoteStatisticItem>>, IQueryable<IGrouping<string, EmoteStatisticItem>>> orderFunc,
        int? skip, int take)
    {
        using var dbContext = DbFactory.Create();

        var query = GetListQuery(dbContext, ofUser?.Id, guild.Id, orderFunc, skip, take);
        return await query.ToListAsync();
    }

    private static IQueryable<EmoteStatItem> GetListQuery(GrillBotContext context, ulong? userId, ulong? guildId,
        Func<IQueryable<IGrouping<string, EmoteStatisticItem>>, IQueryable<IGrouping<string, EmoteStatisticItem>>> orderFunc, int? skip, int? take)
    {
        var query = context.Emotes.AsNoTracking()
            .Where(o => o.GuildId == guildId.ToString());

        if (userId != null)
            query = query.Where(o => o.UserId == userId.ToString());

        var groupQuery = query.GroupBy(o => o.EmoteId);

        if (orderFunc != null)
            groupQuery = orderFunc(groupQuery);

        var resultQuery = groupQuery.Select(o => new EmoteStatItem()
        {
            Id = o.Key,
            UsersCount = o.Count(),
            UseCount = o.Sum(x => x.UseCount),
            FirstOccurence = o.Min(x => x.FirstOccurence),
            LastOccurence = o.Max(x => x.LastOccurence)
        });

        if (skip != null)
            resultQuery = resultQuery.Skip(skip.Value);

        if (take != null)
            resultQuery = resultQuery.Take(take.Value);

        return resultQuery;
    }

    public async Task<List<EmoteStatItem>> GetEmoteListAsync(IGuild guild, IUser ofUser, string sortQuery, int? skip = null, int take = EmbedBuilder.MaxFieldCount)
    {
        return sortQuery switch
        {
            "count/asc" => await GetEmoteListAsync(guild, ofUser, o => o.OrderBy(x => x.Sum(t => t.UseCount)).ThenBy(o => o.Max(x => x.LastOccurence)), skip, take),
            "count/desc" => await GetEmoteListAsync(guild, ofUser, o => o.OrderByDescending(x => x.Sum(t => t.UseCount)).ThenByDescending(o => o.Max(x => x.LastOccurence)), skip, take),
            "lastuse/asc" => await GetEmoteListAsync(guild, ofUser, o => o.OrderBy(x => x.Max(t => t.LastOccurence)).ThenBy(x => x.Sum(t => t.UseCount)), skip, take),
            "lastuse/desc" => await GetEmoteListAsync(guild, ofUser, o => o.OrderByDescending(x => x.Max(t => t.LastOccurence)).ThenByDescending(x => x.Sum(t => t.UseCount)), skip, take),
            _ => throw new NotSupportedException()
        };
    }

    public async Task<int> GetEmotesCountAsync(IGuild guild, IUser ofUser)
    {
        using var context = DbFactory.Create();
        return await GetListQuery(context, ofUser?.Id, guild.Id, null, null, null).CountAsync();
    }
}
