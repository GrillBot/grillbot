using GrillBot.App.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Request.CreateItems;

namespace GrillBot.App.Actions.Api.V1.Reminder;

public class FinishRemind : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }
    private RemindHelper RemindHelper { get; }
    private IDiscordClient DiscordClient { get; }
    private IAuditLogServiceClient AuditLogServiceClient { get; }

    public bool IsGone { get; private set; }
    public bool IsAuthorized { get; private set; }
    public string? ErrorMessage { get; private set; }

    private bool IsCancel => ApiContext.GetUserId() != DiscordClient.CurrentUser.Id;

    public FinishRemind(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IDiscordClient discordClient, ITextsManager texts,
        IAuditLogServiceClient auditLogServiceClient) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
        DiscordClient = discordClient;
        AuditLogServiceClient = auditLogServiceClient;

        RemindHelper = new RemindHelper(discordClient, texts);
    }

    public void ResetState()
    {
        IsGone = false;
        IsAuthorized = false;
        ErrorMessage = null;
    }

    public async Task ProcessAsync(long id, bool notify, bool isService)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var remind = await repository.Remind.FindRemindByIdAsync(id);

        if (remind == null)
            throw new NotFoundException(Texts["RemindModule/CancelRemind/NotFound", ApiContext.Language]);

        CheckAndSetState(remind);
        if (IsGone) return;

        CheckAndSetAuthorization(remind, isService);
        if (!IsAuthorized) return;

        await ProcessRemindAsync(remind, notify);
        await CreateLogItemAsync(remind, notify);
        await repository.CommitAsync();
    }

    private void CheckAndSetState(Database.Entity.RemindMessage message)
    {
        IsGone = !string.IsNullOrEmpty(message.RemindMessageId);

        if (!IsGone) return;
        ErrorMessage = message.RemindMessageId == RemindHelper.NotSentRemind
            ? Texts["RemindModule/CancelRemind/AlreadyCancelled", ApiContext.Language]
            : Texts["RemindModule/CancelRemind/AlreadyNotified", ApiContext.Language];
    }

    private void CheckAndSetAuthorization(Database.Entity.RemindMessage message, bool isService)
    {
        var @operator = ApiContext.LoggedUser!;

        // Only user who created remind, receiver, current bot (in the scheduled job) or authorized user in the web administration can cancel/process notify.
        IsAuthorized = isService || message.FromUserId == @operator.Id.ToString() || message.ToUserId == @operator.Id.ToString() || @operator.Id == DiscordClient.CurrentUser.Id;

        if (!IsAuthorized)
            ErrorMessage = Texts["RemindModule/CancelRemind/InvalidOperator", ApiContext.Language];
    }

    private async Task ProcessRemindAsync(Database.Entity.RemindMessage remind, bool notify)
    {
        if (notify)
            remind.RemindMessageId = await RemindHelper.ProcessRemindAsync(remind, IsCancel);
        else
            remind.RemindMessageId = RemindHelper.NotSentRemind;
    }

    private async Task CreateLogItemAsync(Database.Entity.RemindMessage remind, bool notify)
    {
        // Is not required create log item while processing from scheduled jobs.
        if (!IsCancel) return;

        var logRequest = new LogRequest
        {
            Type = LogType.Info,
            CreatedAt = DateTime.UtcNow,
            LogMessage = new LogMessageRequest
            {
                Message = $"Bylo stornováno upozornění s ID {remind.Id}. {(notify ? "Při rušení bylo odesláno upozornění uživateli." : "")}".Trim(),
                Severity = LogSeverity.Info,
                Source = $"Remind.{nameof(FinishRemind)}",
                SourceAppName = "GrillBot"
            },
            UserId = ApiContext.GetUserId().ToString(),
        };

        await AuditLogServiceClient.CreateItemsAsync(new List<LogRequest> { logRequest });
    }
}
