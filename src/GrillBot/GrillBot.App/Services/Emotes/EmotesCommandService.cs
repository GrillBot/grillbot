using GrillBot.App.Helpers;
using GrillBot.App.Infrastructure;
using GrillBot.App.Modules.Implementations.Emotes;
using GrillBot.Data.Extensions;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Emotes;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Services.Emotes;

public class EmotesCommandService : ServiceBase
{
    private IServiceProvider ServiceProvider { get; }

    public EmotesCommandService(IServiceProvider serviceProvider, GrillBotContextFactory dbFactory,
        IDiscordClient dcClient) : base(null, dbFactory, null, dcClient)
    {
        ServiceProvider = serviceProvider;
    }

    public async Task<Tuple<Embed, long>> GetEmoteStatListEmbedAsync(IInteractionContext context, IUser ofUser, string orderBy, bool descending,
        bool filterAnimated, int page = 1)
    {
        var @params = new EmotesListParams()
        {
            GuildId = context.Guild.Id.ToString(),
            UserId = ofUser?.Id.ToString(),
            FilterAnimated = filterAnimated,
            Sort = new SortParams()
            {
                Descending = descending,
                OrderBy = orderBy
            },
            Pagination = new PaginatedParams()
            {
                Page = page,
                PageSize = EmbedBuilder.MaxFieldCount - 1
            }
        };

        using var scope = ServiceProvider.CreateScope();
        var apiService = scope.ServiceProvider.GetRequiredService<EmotesApiService>();
        var list = await apiService.GetStatsOfEmotesAsync(@params, false, CancellationToken.None);

        return Tuple.Create(
            new EmbedBuilder().WithEmoteList(list.Data, context.User, ofUser, context.Guild, orderBy, descending, page).Build(),
            list.TotalItemsCount
        );
    }

    public async Task<long> GetEmoteStatsCountAsync(IInteractionContext context, IUser ofUser, bool filterAnimated)
    {
        var @params = new EmotesListParams()
        {
            GuildId = context.Guild.Id.ToString(),
            UserId = ofUser?.Id.ToString(),
            FilterAnimated = filterAnimated
        };

        using var scope = ServiceProvider.CreateScope();
        var apiService = scope.ServiceProvider.GetRequiredService<EmotesApiService>();
        var list = await apiService.GetStatsOfEmotesAsync(@params, false, CancellationToken.None);

        return list.TotalItemsCount;
    }

    public async Task<Embed> GetInfoAsync(IEmote emoteItem, IUser caller)
    {
        EnsureEmote(emoteItem, out var emote);

        using var context = DbFactory.Create();
        var baseQuery = context.Emotes.AsNoTracking()
            .Where(o => o.EmoteId == emote.ToString() && o.UseCount > 0);

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

        var guild = await DcClient.GetGuildAsync(Convert.ToUInt64(data.GuildId));
        var topTenQuery = baseQuery.OrderByDescending(x => x.UseCount).ThenByDescending(x => x.LastOccurence).Take(10);

        var topTen = await topTenQuery.AsAsyncEnumerable().SelectAwait(async (o, i) =>
        {
            var user = await DcClient.FindUserAsync(Convert.ToUInt64(o.UserId));
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

    private static void EnsureEmote(IEmote emote, out Emote result)
    {
        if (emote is not Emote _result)
            throw new ArgumentException("Unicode emoji nejsou v tomto příkazu podporovány.");

        result = _result;
    }
}
