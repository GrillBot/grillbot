using GrillBot.App.Infrastructure.Embeds;
using GrillBot.App.Modules.Implementations.Reminder;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models.Pagination;
using GrillBot.Data.Models.API.Reminder;

namespace GrillBot.App.Actions.Commands.Reminder;

public class GetReminderList : CommandAction
{
    private Actions.Api.V1.Reminder.GetReminderList ApiAction { get; }
    private ITextsManager Texts { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public GetReminderList(Actions.Api.V1.Reminder.GetReminderList apiAction, ITextsManager texts, GrillBotDatabaseBuilder databaseBuilder)
    {
        ApiAction = apiAction;
        Texts = texts;
        DatabaseBuilder = databaseBuilder;
    }

    public async Task<(Embed embed, MessageComponent paginationComponent)> ProcessAsync(int page)
    {
        ApiAction.UpdateContext(Locale, Context.User);

        var parameters = GetParameters(page);
        var data = await ApiAction.ProcessAsync(parameters);
        var pagesCount = ComputePagesCount(data.TotalItemsCount);
        var embed = await CreateEmbedAsync(data, page);
        var paginationComponents = ComponentsHelper.CreatePaginationComponents(page, pagesCount, "remind");

        return (embed, paginationComponents);
    }

    public async Task<int> ComputePagesCountAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var parameters = GetParameters(0);
        var count = await repository.Remind.GetRemindersCountAsync(parameters);
        return ComputePagesCount(count);
    }

    private static int ComputePagesCount(long totalCount)
        => (int)Math.Ceiling(totalCount / (double)EmbedBuilder.MaxFieldCount);

    private GetReminderListParams GetParameters(int page)
    {
        return new GetReminderListParams
        {
            Pagination = { Page = page, PageSize = EmbedBuilder.MaxFieldCount },
            Sort = { OrderBy = "At", Descending = true },
            ToUserId = Context.User.Id.ToString(),
            OnlyWaiting = true
        };
    }

    private async Task<Embed> CreateEmbedAsync(PaginatedResponse<RemindMessage> data, int page)
    {
        var forUser = Context.User.GetDisplayName();
        var embed = new EmbedBuilder()
            .WithFooter(Context.User)
            .WithMetadata(new RemindListMetadata { Page = page })
            .WithAuthor(Texts["RemindModule/List/Embed/Title", Locale].FormatWith(forUser))
            .WithColor(Color.Blue)
            .WithCurrentTimestamp();

        if (data.TotalItemsCount == 0)
        {
            embed.WithDescription(Texts["RemindModule/List/Embed/NoItems", Locale].FormatWith(forUser));
        }
        else
        {
            foreach (var remind in data.Data)
            {
                var fromUser = (await Context.Client.FindUserAsync(remind.FromUser.Id.ToUlong()))?.GetDisplayName();
                var at = remind.At.ToCzechFormat();
                var timeTo = (DateTime.Now - remind.At).Humanize(culture: Texts.GetCulture(Locale));
                var title = Texts["RemindModule/List/Embed/RowTitle", Locale].FormatWith(remind.Id, fromUser, at, timeTo);
                var message = remind.Message[..Math.Min(remind.Message.Length, EmbedFieldBuilder.MaxFieldValueLength)];

                embed.AddField(title, message);
            }
        }

        return embed.Build();
    }
}
