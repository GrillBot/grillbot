using GrillBot.App.Infrastructure.Embeds;
using GrillBot.App.Modules.Implementations.Reminder;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Extensions;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Core.Services.RemindService;
using GrillBot.Core.Services.RemindService.Models.Request;
using GrillBot.Core.Services.RemindService.Models.Response;

namespace GrillBot.App.Actions.Commands.Reminder;

public class GetReminderList(
    ITextsManager _texts,
    IServiceClientExecutor<IRemindServiceClient> _remindService
) : CommandAction
{
    private string ForUser
        => Context.User.GetDisplayName();

    public async Task<(Embed embed, MessageComponent? paginationComponent)> ProcessAsync(int page)
    {
        var embed = CreateEmptyEmbed(page);
        var request = CreateRequest();
        var list = await _remindService.ExecuteRequestAsync((c, ctx) => c.GetReminderListAsync(request, ctx.CancellationToken));
        var fields = await CreateFieldsAsync(list);
        SetPage(embed, fields, page, out var pagesCount);

        var paginationComponents = ComponentsHelper.CreatePaginationComponents(page, pagesCount, "remind");
        return (embed.Build(), paginationComponents);
    }

    public async Task<int> ComputePagesCountAsync()
    {
        var request = CreateRequest();
        var list = await _remindService.ExecuteRequestAsync((c, ctx) => c.GetReminderListAsync(request, ctx.CancellationToken));
        var fields = await CreateFieldsAsync(list);
        var embed = CreateEmptyEmbed(0);

        return EmbedHelper.SplitToPages(fields, embed).Count;
    }

    private ReminderListRequest CreateRequest()
    {
        return new ReminderListRequest
        {
            Pagination = { Page = 0, PageSize = int.MaxValue },
            Sort = { OrderBy = "NotifyAt", Descending = true },
            ToUserId = Context.User.Id.ToString(),
            OnlyPending = true
        };
    }

    private async Task<List<EmbedFieldBuilder>> CreateFieldsAsync(PaginatedResponse<RemindMessageItem> list)
    {
        var result = new List<EmbedFieldBuilder>();
        if (list.TotalItemsCount == 0)
            return result;

        var now = DateTime.Now;
        var titleTemplate = _texts["RemindModule/List/Embed/RowTitle", Locale];
        var culture = _texts.GetCulture(Locale);

        foreach (var remind in list.Data)
        {
            var fromUser = await Context.Client.FindUserAsync(remind.FromUserId.ToUlong());
            var notifyAt = remind.NotifyAtUtc.ToLocalTime();
            var at = notifyAt.ToCzechFormat();
            var remaining = (now - notifyAt).Humanize(culture: culture);
            var title = titleTemplate.FormatWith(remind.Id, fromUser?.GetDisplayName(), at, remaining);
            var message = remind.Message[..Math.Min(remind.Message.Length, EmbedFieldBuilder.MaxFieldValueLength)];

            result.Add(new EmbedFieldBuilder().WithName(title).WithValue(message));
        }

        return result;
    }

    private EmbedBuilder CreateEmptyEmbed(int page)
    {
        return new EmbedBuilder()
            .WithFooter(Context.User)
            .WithMetadata(new RemindListMetadata { Page = page })
            .WithAuthor(_texts["RemindModule/List/Embed/Title", Locale].FormatWith(ForUser))
            .WithColor(Color.Blue)
            .WithCurrentTimestamp();
    }

    private void SetPage(EmbedBuilder embed, IReadOnlyList<EmbedFieldBuilder> fields, int page, out int pagesCount)
    {
        if (fields.Count == 0)
        {
            embed.WithDescription(_texts["RemindModule/List/Embed/NoItems", Locale].FormatWith(ForUser));
            pagesCount = 1;
            return;
        }

        var pages = EmbedHelper.SplitToPages(fields, embed);
        pagesCount = pages.Count;
        embed.WithFields(pages[page]);
    }
}
