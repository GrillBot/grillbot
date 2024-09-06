using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.RemindService;
using GrillBot.Core.Services.RemindService.Models.Request;
using GrillBot.Data.Models.API;
using Microsoft.AspNetCore.Http;

namespace GrillBot.App.Actions.Api.V1.Reminder;

public class FinishRemind : ApiAction
{
    private readonly IRemindServiceClient _remindService;
    private readonly ITextsManager _texts;

    public bool IsGone { get; private set; }
    public bool IsAuthorized { get; private set; }
    public string? ErrorMessage { get; private set; }

    public FinishRemind(ApiRequestContext apiContext, ITextsManager texts, IRemindServiceClient remindService) : base(apiContext)
    {
        _texts = texts;
        _remindService = remindService;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var id = GetParameter<long>(0);
        var notify = GetParameter<bool>(1);
        var isService = GetParameter<bool>(2);

        await ProcessAsync(id, notify, isService);
        return IsGone ?
            new ApiResult(StatusCodes.Status410Gone, new MessageResponse(ErrorMessage!)) :
            ApiResult.Ok();
    }

    public async Task ProcessAsync(long id, bool notify, bool isService)
    {
        try
        {
            var request = new CancelReminderRequest
            {
                ExecutingUserId = ApiContext.GetUserId().ToString(),
                IsAdminExecution = isService,
                NotifyUser = notify,
                RemindId = id
            };

            await _remindService.CancelReminderAsync(request);

            IsGone = false;
            IsAuthorized = true;
        }
        catch (ClientBadRequestException ex)
        {
            ProcessRemindServiceErrors(ex);
        }
    }

    private void ProcessRemindServiceErrors(ClientBadRequestException exception)
    {
        foreach (var (key, errors) in exception.ValidationErrors)
        {
            if (key == "RemindId")
            {
                var notFoundError = Array.Find(errors, e => e.EndsWith("NotFound"));
                if (!string.IsNullOrEmpty(notFoundError))
                    throw new NotFoundException(_texts[notFoundError, ApiContext.Language]);
            }

            if (key != "Remind")
                continue;

            var alreadyNotified = Array.Find(errors, e => e.EndsWith("AlreadyNotified"));
            var alreadyCancelled = Array.Find(errors, e => e.EndsWith("AlreadyCancelled"));
            var remindInProgress = Array.Find(errors, e => e.EndsWith("RemindInProgress"));

            IsGone = !string.IsNullOrEmpty(alreadyNotified) || !string.IsNullOrEmpty(alreadyCancelled) || !string.IsNullOrEmpty(remindInProgress);
            if (IsGone)
            {
                if (!string.IsNullOrEmpty(alreadyCancelled))
                    ErrorMessage = _texts[alreadyCancelled, ApiContext.Language];
                else if (!string.IsNullOrEmpty(remindInProgress))
                    ErrorMessage = _texts[remindInProgress, ApiContext.Language];
                else if (!string.IsNullOrEmpty(alreadyNotified))
                    ErrorMessage = _texts[alreadyNotified, ApiContext.Language];

                return;
            }

            var invalidOperator = Array.Find(errors, e => e.EndsWith("InvalidOperator"));
            IsAuthorized = string.IsNullOrEmpty(invalidOperator);
            if (!IsAuthorized)
            {
                ErrorMessage = _texts[invalidOperator!, ApiContext.Language];
                return;
            }
        }

        throw exception;
    }
}
