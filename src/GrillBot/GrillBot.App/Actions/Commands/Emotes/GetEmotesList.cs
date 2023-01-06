using GrillBot.App.Infrastructure.Embeds;
using GrillBot.App.Modules.Implementations.Emotes;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Data.Models.API.Emotes;
using GrillBot.Database.Models;

namespace GrillBot.App.Actions.Commands.Emotes;

public class GetEmotesList : CommandAction
{
    private Api.V1.Emote.GetStatsOfEmotes ApiAction { get; }
    private ITextsManager Texts { get; }

    public GetEmotesList(Api.V1.Emote.GetStatsOfEmotes apiAction, ITextsManager texts)
    {
        ApiAction = apiAction;
        Texts = texts;
    }

    public async Task<(Embed embed, MessageComponent paginationComponent)> ProcessAsync(int page, string sort, bool descending, IUser ofUser, bool filterAnimated)
    {
        var parameters = CreateParameters(page, sort, descending, ofUser, filterAnimated);
        var list = await ApiAction.ProcessAsync(parameters, false);
        var embed = CreateEmbed(list, sort, descending, filterAnimated, ofUser);
        var pagesCount = ComputePagesCount(list.TotalItemsCount);
        var paginationComponent = ComponentsHelper.CreatePaginationComponents(page, pagesCount, "emote");

        return (embed, paginationComponent);
    }

    public async Task<int> ComputePagesCountAsync(string sort, bool descending, IUser ofUser, bool filterAnimated)
    {
        var parameters = CreateParameters(0, sort, descending, ofUser, filterAnimated);
        var list = await ApiAction.ProcessAsync(parameters, false);
        return ComputePagesCount(list.TotalItemsCount);
    }

    private static int ComputePagesCount(long totalCount) =>
        (int)Math.Ceiling(totalCount / (double)(EmbedBuilder.MaxFieldCount - 1));

    private EmotesListParams CreateParameters(int page, string sort, bool descending, IUser ofUser, bool filterAnimated)
    {
        return new EmotesListParams
        {
            GuildId = Context.Guild.Id.ToString(),
            UserId = ofUser?.Id.ToString(),
            FilterAnimated = filterAnimated,
            Sort =
            {
                Descending = descending,
                OrderBy = sort
            },
            Pagination =
            {
                Page = page,
                PageSize = EmbedBuilder.MaxFieldCount - 1
            }
        };
    }

    private Embed CreateEmbed(PaginatedResponse<EmoteStatItem> list, string sort, bool descending, bool filterAnimated, IUser ofUser)
    {
        var embed = new EmbedBuilder()
            .WithFooter(Context.User)
            .WithMetadata(new EmoteListMetadata
            {
                Descending = descending,
                OrderBy = sort,
                Page = list.Page,
                FilterAnimated = filterAnimated,
                OfUserId = ofUser?.Id
            })
            .WithAuthor(Texts["Emote/List/Title", Locale])
            .WithColor(Color.Blue)
            .WithCurrentTimestamp();

        if (list.TotalItemsCount == 0)
        {
            var description = ofUser != null ? Texts["Emote/List/NoStatsOfUser", Locale].FormatWith(ofUser.GetFullName()) : Texts["Emote/List/NoStats", Locale];
            embed.WithDescription(description);
        }
        else
        {
            foreach (var item in list.Data)
            {
                var data = Texts["Emote/List/FieldData", Locale].FormatWith(item.UseCount, item.UsedUsersCount, item.FirstOccurence.ToCzechFormat(), item.LastOccurence.ToCzechFormat());
                embed.AddField(item.Emote.FullId, data, true);
            }
        }

        return embed.Build();
    }
}
