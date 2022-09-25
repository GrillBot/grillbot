using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Reminder;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Actions.Api.V1.Reminder;

public class CancelRemind : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private AuditLogWriter AuditLogWriter { get; }
    private ITextsManager Texts { get; }
    private RemindHelper RemindHelper { get; }

    public bool IsGone { get; private set; }
    public bool IsAuthorized { get; private set; }
    public string ErrorMessage { get; private set; }

    public CancelRemind(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, AuditLogWriter auditLogWriter, IDiscordClient discordClient, ITextsManager texts) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        AuditLogWriter = auditLogWriter;
        Texts = texts;

        RemindHelper = new RemindHelper(discordClient, texts);
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
        IsAuthorized = isService || message.FromUserId == @operator.Id.ToString() || message.ToUserId == @operator.Id.ToString();

        if (!IsAuthorized)
            ErrorMessage = Texts["RemindModule/CancelRemind/InvalidOperator", ApiContext.Language];
    }

    private async Task ProcessRemindAsync(Database.Entity.RemindMessage remind, bool notify)
    {
        if (notify)
            remind.RemindMessageId = await RemindHelper.ProcessRemindAsync(remind, true);
        else
            remind.RemindMessageId = RemindHelper.NotSentRemind;
    }

    private async Task CreateLogItemAsync(Database.Entity.RemindMessage remind, bool notify)
    {
        var logItem = new AuditLogDataWrapper(AuditLogItemType.Info, $"Bylo stornováno upozornění s ID {remind.Id}. {(notify ? "Při rušení bylo odesláno upozornění uživateli." : "")}".Trim(), null,
            null, ApiContext.LoggedUser);
        await AuditLogWriter.StoreAsync(logItem);
    }
}
