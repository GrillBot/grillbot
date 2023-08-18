using GrillBot.App.Infrastructure.Embeds;
using GrillBot.App.Modules.Implementations.Reminder;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Extensions;
using GrillBot.Core.Models.Pagination;
using GrillBot.Data.Models.API.Reminder;

namespace GrillBot.App.Actions.Commands.Reminder;

public class GetReminderList : CommandAction
{
    private Api.V1.Reminder.GetReminderList ApiAction { get; }
    private ITextsManager Texts { get; }

    private string ForUser
        => Context.User.GetDisplayName();

    public GetReminderList(Api.V1.Reminder.GetReminderList apiAction, ITextsManager texts)
    {
        ApiAction = apiAction;
        Texts = texts;
    }

    public async Task<(Embed embed, MessageComponent? paginationComponent)> ProcessAsync(int page)
    {
        ApiAction.UpdateContext(Locale, Context.User);

        var embed = CreateEmptyEmbed(page);
        var parameters = CreateParameters();
        var list = await ApiAction.ProcessAsync(parameters);
        var fields = await CreateFieldsAsync(list);
        SetPage(embed, fields, page, out var pagesCount);

        var paginationComponents = ComponentsHelper.CreatePaginationComponents(page, pagesCount, "remind");
        return (embed.Build(), paginationComponents);
    }

    public async Task<int> ComputePagesCountAsync()
    {
        ApiAction.UpdateContext(Locale, Context.User);

        var parameters = CreateParameters();
        var list = await ApiAction.ProcessAsync(parameters);
        var fields = await CreateFieldsAsync(list);
        var embed = CreateEmptyEmbed(0);

        return EmbedHelper.SplitToPages(fields, embed).Count;
    }

    private GetReminderListParams CreateParameters()
    {
        return new GetReminderListParams
        {
            Pagination = { Page = 0, PageSize = int.MaxValue },
            Sort = { OrderBy = "At", Descending = true },
            ToUserId = Context.User.Id.ToString(),
            OnlyWaiting = true
        };
    }

    private async Task<List<EmbedFieldBuilder>> CreateFieldsAsync(PaginatedResponse<RemindMessage> list)
    {
        var result = new List<EmbedFieldBuilder>();
        if (list.TotalItemsCount == 0)
            return result;

        var now = DateTime.Now;
        var titleTemplate = Texts["RemindModule/List/Embed/RowTitle", Locale];

        foreach (var remind in list.Data)
        {
            var fromUser = await Context.Client.FindUserAsync(remind.FromUser.Id.ToUlong());
            var at = remind.At.ToCzechFormat();
            var remaining = (now - remind.At).Humanize(culture: Texts.GetCulture(Locale));
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
            .WithAuthor(Texts["RemindModule/List/Embed/Title", Locale].FormatWith(ForUser))
            .WithColor(Color.Blue)
            .WithCurrentTimestamp();
    }

    private void SetPage(EmbedBuilder embed, IReadOnlyList<EmbedFieldBuilder> fields, int page, out int pagesCount)
    {
        if (fields.Count == 0)
        {
            embed.WithDescription(Texts["RemindModule/List/Embed/NoItems", Locale].FormatWith(ForUser));
            pagesCount = 1;
            return;
        }

        var pages = EmbedHelper.SplitToPages(fields, embed);
        pagesCount = pages.Count;
        embed.WithFields(pages[page]);
    }
}
