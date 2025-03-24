using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Services.Common.Exceptions;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Core.Services.RemindService;
using GrillBot.Core.Services.RemindService.Models.Request;

namespace GrillBot.App.Actions.Commands.Reminder;

public class FinishRemind(
    IServiceClientExecutor<IRemindServiceClient> _remindService,
    ITextsManager _texts
) : CommandAction
{
    public bool IsGone { get; private set; }
    public bool IsAuthorized { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task ProcessAsync(long id, bool notify, bool isService)
    {
        try
        {
            var request = new CancelReminderRequest
            {
                ExecutingUserId = Context.User.Id.ToString(),
                IsAdminExecution = isService,
                NotifyUser = notify,
                RemindId = id
            };

            await _remindService.ExecuteRequestAsync((c, cancellationToken) => c.CancelReminderAsync(request, cancellationToken));

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
                    throw new NotFoundException(_texts[notFoundError, Locale]);
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
                    ErrorMessage = _texts[alreadyCancelled, Locale];
                else if (!string.IsNullOrEmpty(remindInProgress))
                    ErrorMessage = _texts[remindInProgress, Locale];
                else if (!string.IsNullOrEmpty(alreadyNotified))
                    ErrorMessage = _texts[alreadyNotified, Locale];

                return;
            }

            var invalidOperator = Array.Find(errors, e => e.EndsWith("InvalidOperator"));
            IsAuthorized = string.IsNullOrEmpty(invalidOperator);
            if (!IsAuthorized)
            {
                ErrorMessage = _texts[invalidOperator!, Locale];
                return;
            }
        }

        throw exception;
    }
}
