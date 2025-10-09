using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Services.Common.Exceptions;
using GrillBot.Core.Services.Common.Executor;
using RemindService;
using RemindService.Models.Request;

namespace GrillBot.App.Actions.Commands.Reminder;

public class CopyRemind(
    ITextsManager _texts,
    IServiceClientExecutor<IRemindServiceClient> _remindService
) : CommandAction
{
    public async Task ProcessAsync(long originalRemindId)
    {
        var request = new CopyReminderRequest
        {
            Language = Locale,
            RemindId = originalRemindId,
            ToUserId = Context.User.Id.ToString()
        };

        try
        {
            await _remindService.ExecuteRequestAsync((c, ctx) => c.CopyReminderAsync(request, ctx.CancellationToken));
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

            var selfCopy = Array.Find(errors, e => e.EndsWith("SelfCopy"));
            if (!string.IsNullOrEmpty(selfCopy))
                throw new ValidationException(_texts[selfCopy, Locale]);

            var copyExists = Array.Find(errors, e => e.EndsWith("CopyExists"));
            if (!string.IsNullOrEmpty(copyExists))
                throw new ValidationException(_texts[copyExists, Locale]);

            var wasCancelled = Array.Find(errors, e => e.EndsWith("WasCancelled"));
            if (!string.IsNullOrEmpty(wasCancelled))
                throw new ValidationException(_texts[wasCancelled, Locale]);

            var wasSent = Array.Find(errors, e => e.EndsWith("WasSent"));
            if (!string.IsNullOrEmpty(wasSent))
                throw new ValidationException(_texts[wasSent, Locale]);
        }

        throw exception;
    }
}
