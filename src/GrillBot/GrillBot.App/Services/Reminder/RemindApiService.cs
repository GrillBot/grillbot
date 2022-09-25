using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Models;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.Reminder;

public class RemindApiService
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ApiRequestContext ApiRequestContext { get; }
    private RemindService RemindService { get; }
    private AuditLogWriter AuditLogWriter { get; }

    public RemindApiService(GrillBotDatabaseBuilder databaseBuilder, ApiRequestContext apiRequestContext,
        RemindService remindService, AuditLogWriter auditLogWriter)
    {
        DatabaseBuilder = databaseBuilder;
        ApiRequestContext = apiRequestContext;
        RemindService = remindService;
        AuditLogWriter = auditLogWriter;
    }

    /// <summary>
    /// Service cancellation of remind.
    /// </summary>
    public async Task ServiceCancellationAsync(long id, bool notify = false)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var remind = await repository.Remind.FindRemindByIdAsync(id);

        if (remind == null)
            throw new NotFoundException("Požadované upozornění neexistuje.");

        if (!string.IsNullOrEmpty(remind.RemindMessageId))
        {
            if (remind.RemindMessageId == "0")
                throw new InvalidOperationException("Nelze zrušit již zrušené upozornění.");

            throw new InvalidOperationException("Nelze zrušit již proběhlé upozornění.");
        }

        ulong messageId = 0;
        if (notify)
            messageId = await RemindService.SendNotificationMessageAsync(remind, true);

        var logItem = new AuditLogDataWrapper(
            AuditLogItemType.Info,
            $"Bylo stornováno upozornění s ID {id}. {(notify ? "Při rušení bylo odesláno upozornění uživateli." : "")}".Trim(),
            null, null, ApiRequestContext.LoggedUser
        );
        await AuditLogWriter.StoreAsync(logItem);

        remind.RemindMessageId = messageId.ToString();
        await repository.CommitAsync();
    }
}
