using GrillBot.App.Infrastructure.Embeds;
using GrillBot.App.Modules.Implementations.Emotes;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Extensions;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Core.Services.Emote;
using GrillBot.Core.Services.Emote.Models.Request;
using GrillBot.Core.Services.Emote.Models.Response;
using GrillBot.Data.Enums;

namespace GrillBot.App.Actions.Commands.Emotes;

public class GetEmotesList(
    ITextsManager _texts,
    IServiceClientExecutor<IEmoteServiceClient> _client
) : CommandAction
{
    public async Task<(Embed embed, MessageComponent? paginationComponent)> ProcessAsync(int page, string sort, SortType sortType, IUser? ofUser, bool filterAnimated)
    {
        var parameters = CreateParameters(page, sort, sortType, ofUser, filterAnimated, false);
        var statistics = await _client.ExecuteRequestAsync((c, cancellationToken) => c.GetEmoteStatisticsListAsync(parameters, cancellationToken));
        var embed = CreateEmbed(statistics, sort, sortType, filterAnimated, ofUser);
        var pagesCount = ComputePagesCount(statistics.TotalItemsCount);
        var paginationComponent = ComponentsHelper.CreatePaginationComponents(page, pagesCount, "emote");

        return (embed, paginationComponent);
    }

    public async Task<int> ComputePagesCountAsync(string sort, SortType sortType, IUser? ofUser, bool filterAnimated)
    {
        var parameters = CreateParameters(0, sort, sortType, ofUser, filterAnimated, true);
        var statistics = await _client.ExecuteRequestAsync((c, cancellationToken) => c.GetEmoteStatisticsListAsync(parameters, cancellationToken));
        return ComputePagesCount(statistics.TotalItemsCount);
    }

    private static int ComputePagesCount(long totalCount) =>
        (int)Math.Ceiling(totalCount / (double)(EmbedBuilder.MaxFieldCount - 1));

    private EmoteStatisticsListRequest CreateParameters(int page, string sort, SortType sortType, IUser? ofUser, bool ignoreAnimated,
        bool onlyCount)
    {
        return new EmoteStatisticsListRequest
        {
            GuildId = Context.Guild.Id.ToString(),
            UserId = ofUser?.Id.ToString(),
            IgnoreAnimated = ignoreAnimated,
            Unsupported = false,
            Pagination =
            {
                Page = page,
                PageSize = EmbedBuilder.MaxFieldCount - 1,
                OnlyCount = onlyCount
            },
            Sort =
            {
                Descending = sortType == SortType.Descending,
                OrderBy = sort
            }
        };
    }

    private Embed CreateEmbed(PaginatedResponse<EmoteStatisticsItem> list, string sort, SortType sortType, bool filterAnimated, IUser? ofUser)
    {
        var embed = new EmbedBuilder()
            .WithFooter(Context.User)
            .WithMetadata(new EmoteListMetadata
            {
                SortType = sortType,
                OrderBy = sort,
                Page = list.Page,
                FilterAnimated = filterAnimated,
                OfUserId = ofUser?.Id
            })
            .WithAuthor(_texts["Emote/List/Title", Locale])
            .WithColor(Color.Blue)
            .WithCurrentTimestamp();

        if (list.TotalItemsCount == 0)
        {
            var description = ofUser != null ?
                _texts["Emote/List/NoStatsOfUser", Locale].FormatWith(ofUser.GetFullName()) :
                _texts["Emote/List/NoStats", Locale];

            embed.WithDescription(description);
        }
        else
        {
            foreach (var item in list.Data)
            {
                var data = _texts["Emote/List/FieldData", Locale]
                    .FormatWith(item.UseCount, item.UsersCount, item.FirstOccurence.ToLocalTime().ToCzechFormat(), item.LastOccurence.ToLocalTime().ToCzechFormat());

                var emote = new Emote(item.EmoteId.ToUlong(), item.EmoteName, item.EmoteIsAnimated);
                embed.AddField(emote.ToString(), data, true);
            }
        }

        return embed.Build();
    }
}
