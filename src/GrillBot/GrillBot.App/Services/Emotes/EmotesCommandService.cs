using GrillBot.App.Modules.Implementations.Emotes;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Data.Models.API.Emotes;
using GrillBot.Database.Models;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Services.Emotes;

public class EmotesCommandService
{
    private IServiceProvider ServiceProvider { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }

    public EmotesCommandService(IServiceProvider serviceProvider, GrillBotDatabaseBuilder databaseBuilder,
        IDiscordClient discordClient)
    {
        ServiceProvider = serviceProvider;
        DatabaseBuilder = databaseBuilder;
        DiscordClient = discordClient;
    }

    public async Task<Tuple<Embed, long>> GetEmoteStatListEmbedAsync(IInteractionContext context, IUser ofUser, string orderBy, bool descending,
        bool filterAnimated, int page = 1)
    {
        var @params = new EmotesListParams
        {
            GuildId = context.Guild.Id.ToString(),
            UserId = ofUser?.Id.ToString(),
            FilterAnimated = filterAnimated,
            Sort = new SortParams
            {
                Descending = descending,
                OrderBy = orderBy
            },
            Pagination = new PaginatedParams
            {
                Page = page,
                PageSize = EmbedBuilder.MaxFieldCount - 1
            }
        };

        using var scope = ServiceProvider.CreateScope();
        var apiService = scope.ServiceProvider.GetRequiredService<Actions.Api.V1.Emote.GetStatsOfEmotes>();
        var list = await apiService.ProcessAsync(@params, false);

        return Tuple.Create(
            new EmbedBuilder().WithEmoteList(list.Data, context.User, ofUser, context.Guild, orderBy, descending, page).Build(),
            list.TotalItemsCount
        );
    }

    public async Task<long> GetEmoteStatsCountAsync(IInteractionContext context, IUser ofUser, bool filterAnimated)
    {
        var @params = new EmotesListParams
        {
            GuildId = context.Guild.Id.ToString(),
            UserId = ofUser?.Id.ToString(),
            FilterAnimated = filterAnimated
        };

        using var scope = ServiceProvider.CreateScope();
        var apiService = scope.ServiceProvider.GetRequiredService<Actions.Api.V1.Emote.GetStatsOfEmotes>();
        var list = await apiService.ProcessAsync(@params, false);

        return list.TotalItemsCount;
    }

    public async Task<Embed> GetInfoAsync(IEmote emoteItem, IUser caller)
    {
        EnsureEmote(emoteItem, out var emote);

        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.Emote.GetStatisticsOfEmoteAsync(emote);
        if (data == null)
            return null;

        var guild = await DiscordClient.GetGuildAsync(data.GuildId.ToUlong());
        var topTenData = await repository.Emote.GetTopUsersOfUsage(emote, 10);
        var topTen = new List<string>();

        for (var i = 0; i < Math.Min(10, topTenData.Count); i++)
        {
            var stat = topTenData[i];
            var user = await DiscordClient.FindUserAsync(stat.UserId.ToUlong());
            topTen.Add($"**{i + 1,2}.** {user?.GetDisplayName() ?? "Neznámý uživatel"} ({stat.UseCount})");
        }

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
            .AddField("Počet uživatelů", data.UsedUsersCount, true)
            .AddField("Server", guild?.Name ?? "Neznámý server", true)
            .AddField("TOP 10 použití", string.Join("\n", topTen))
            .AddField("Odkaz", emote.Url)
            .WithThumbnailUrl(emote.Url);

        return embed.Build();
    }

    private static void EnsureEmote(IEmote emote, out Emote result)
    {
        if (emote is not Emote resultData)
            throw new ArgumentException("Unicode emoji nejsou v tomto příkazu podporovány.");

        result = resultData;
    }
}
