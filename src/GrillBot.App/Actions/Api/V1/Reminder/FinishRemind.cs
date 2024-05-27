using GrillBot.App.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Events.Create;
using GrillBot.Data.Models.API;
using GrillBot.Database.Entity;
using Microsoft.AspNetCore.Http;

namespace GrillBot.App.Actions.Api.V1.Reminder;

public class FinishRemind : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }
    private RemindHelper RemindHelper { get; }
    private IDiscordClient DiscordClient { get; }

    private readonly IRabbitMQPublisher _rabbitPublisher;

    public bool IsGone { get; private set; }
    public bool IsAuthorized { get; private set; }
    public string? ErrorMessage { get; private set; }
    public RemindMessage? Remind { get; private set; }

    private bool IsCancel => ApiContext.GetUserId() != DiscordClient.CurrentUser.Id;

    public FinishRemind(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IDiscordClient discordClient, ITextsManager texts,
        IRabbitMQPublisher rabbitPublisher) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
        DiscordClient = discordClient;
        _rabbitPublisher = rabbitPublisher;

        RemindHelper = new RemindHelper(discordClient, texts);
    }

    public void ResetState()
    {
        IsGone = false;
        IsAuthorized = false;
        ErrorMessage = null;
        Remind = null;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var id = (long)Parameters[0]!;
        var notify = (bool)Parameters[1]!;
        var isService = (bool)Parameters[2]!;

        await ProcessAsync(id, notify, isService);
        return IsGone ? new ApiResult(StatusCodes.Status410Gone, new MessageResponse(ErrorMessage!)) : ApiResult.Ok();
    }

    public async Task ProcessAsync(long id, bool notify, bool isService)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        Remind = await repository.Remind.FindRemindByIdAsync(id);
        if (Remind == null)
            throw new NotFoundException(Texts["RemindModule/CancelRemind/NotFound", ApiContext.Language]);

        CheckAndSetState();
        if (IsGone) return;

        CheckAndSetAuthorization(isService);
        if (!IsAuthorized) return;

        await ProcessRemindAsync(notify);
        await WriteToAuditLogAsync(notify);
        await repository.CommitAsync();
    }

    private void CheckAndSetState()
    {
        if (Remind is null)
            return;

        IsGone = !string.IsNullOrEmpty(Remind.RemindMessageId);

        if (!IsGone) return;
        ErrorMessage = Remind.RemindMessageId == RemindHelper.NotSentRemind
            ? Texts["RemindModule/CancelRemind/AlreadyCancelled", ApiContext.Language]
            : Texts["RemindModule/CancelRemind/AlreadyNotified", ApiContext.Language];
    }

    private void CheckAndSetAuthorization(bool isService)
    {
        if (Remind is null)
            return;

        var @operator = ApiContext.LoggedUser!;

        // Only user who created remind, receiver, current bot (in the scheduled job) or authorized user in the web administration can cancel/process notify.
        IsAuthorized = isService || Remind.FromUserId == @operator.Id.ToString() || Remind.ToUserId == @operator.Id.ToString() || @operator.Id == DiscordClient.CurrentUser.Id;

        if (!IsAuthorized)
            ErrorMessage = Texts["RemindModule/CancelRemind/InvalidOperator", ApiContext.Language];
    }

    private async Task ProcessRemindAsync(bool notify)
    {
        if (Remind is null)
            return;

        if (notify)
            Remind.RemindMessageId = await RemindHelper.ProcessRemindAsync(Remind, IsCancel);
        else
            Remind.RemindMessageId = RemindHelper.NotSentRemind;
    }

    private async Task WriteToAuditLogAsync(bool notify)
    {
        // Is not required create log item while processing from scheduled jobs.
        if (!IsCancel || Remind is null) return;

        var userId = ApiContext.GetUserId().ToString();
        var logRequest = new LogRequest(LogType.Info, DateTime.UtcNow, null, userId)
        {
            LogMessage = new LogMessageRequest
            {
                Message = $"Bylo stornováno upozornění s ID {Remind.Id}. {(notify ? "Při rušení bylo odesláno upozornění uživateli." : "")}".Trim(),
                Severity = LogSeverity.Info,
                Source = $"Remind.{nameof(FinishRemind)}",
                SourceAppName = "GrillBot"
            }
        };

        await _rabbitPublisher.PublishAsync(new CreateItemsPayload(logRequest), new());
    }
}
