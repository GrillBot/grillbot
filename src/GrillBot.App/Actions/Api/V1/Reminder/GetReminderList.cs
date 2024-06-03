using GrillBot.App.Managers.DataResolve;
using GrillBot.Common.Models;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Models;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.RemindService;
using GrillBot.Core.Services.RemindService.Models.Request;
using GrillBot.Core.Services.RemindService.Models.Response;
using GrillBot.Data.Models.API.Reminder;

namespace GrillBot.App.Actions.Api.V1.Reminder;

public class GetReminderList : ApiAction
{
    private readonly IRemindServiceClient _remindService;
    private readonly DataResolveManager _dataResolve;

    public GetReminderList(ApiRequestContext apiContext, IRemindServiceClient remindService, DataResolveManager dataResolve) : base(apiContext)
    {
        _remindService = remindService;
        _dataResolve = dataResolve;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = (GetReminderListParams)Parameters[0]!;
        var result = await ProcessAsync(parameters);

        return ApiResult.Ok(result);
    }

    public async Task<PaginatedResponse<RemindMessage>> ProcessAsync(GetReminderListParams parameters)
    {
        CheckAndSetPublicAccess(parameters);

        var request = new ReminderListRequest
        {
            CommandMessageId = parameters.OriginalMessageId,
            FromUserId = parameters.FromUserId,
            MessageContains = parameters.MessageContains,
            NotifyAtFromUtc = parameters.CreatedFrom.HasValue ? parameters.CreatedFrom.Value.WithKind(DateTimeKind.Local).ToUniversalTime() : null,
            NotifyAtToUtc = parameters.CreatedTo.HasValue ? parameters.CreatedTo.Value.WithKind(DateTimeKind.Local).ToUniversalTime() : null,
            OnlyInProcess = null,
            OnlyPending = parameters.OnlyWaiting ? true : null,
            Pagination = parameters.Pagination,
            Sort = new SortParameters
            {
                Descending = parameters.Sort.Descending,
                OrderBy = parameters.Sort.OrderBy
            },
            ToUserId = parameters.ToUserId
        };

        var data = await _remindService.GetReminderListAsync(request);
        return await PaginatedResponse<RemindMessage>.CopyAndMapAsync(data, MapItemFromServiceAsync);
    }

    private async Task<RemindMessage> MapItemFromServiceAsync(RemindMessageItem item)
    {
        var fromUser = await _dataResolve.GetUserAsync(item.FromUserId.ToUlong());
        var toUser = await _dataResolve.GetUserAsync(item.ToUserId.ToUlong());

        return new RemindMessage
        {
            At = item.NotifyAtUtc.ToLocalTime(),
            FromUser = fromUser!,
            Id = item.Id,
            Language = item.Language,
            Message = item.Message,
            Notified = !string.IsNullOrEmpty(item.NotificationMessageId) && !item.IsSendInProgress,
            Postpone = item.PostponeCount,
            ToUser = toUser!
        };
    }

    private void CheckAndSetPublicAccess(GetReminderListParams parameters)
    {
        if (!ApiContext.IsPublic()) return;

        parameters.ToUserId = ApiContext.GetUserId().ToString();
        parameters.OriginalMessageId = null;

        if (parameters.Sort.OrderBy == "ToUser")
            parameters.Sort.OrderBy = "Id";
    }
}
