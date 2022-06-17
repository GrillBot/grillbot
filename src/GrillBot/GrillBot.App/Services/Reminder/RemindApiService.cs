using AutoMapper;
using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Models;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API.Reminder;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using GrillBot.Database.Models;

namespace GrillBot.App.Services.Reminder;

public class RemindApiService
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }
    private ApiRequestContext ApiRequestContext { get; }
    private RemindService RemindService { get; }
    private AuditLogService AuditLogService { get; }

    public RemindApiService(GrillBotDatabaseBuilder databaseBuilder, IMapper mapper, ApiRequestContext apiRequestContext,
        RemindService remindService, AuditLogService auditLogService)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        ApiRequestContext = apiRequestContext;
        RemindService = remindService;
        AuditLogService = auditLogService;
    }

    public async Task<PaginatedResponse<RemindMessage>> GetListAsync(GetReminderListParams parameters)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.Remind.GetRemindListAsync(parameters, parameters.Pagination);
        return await PaginatedResponse<RemindMessage>.CopyAndMapAsync(data, entity => Task.FromResult(Mapper.Map<RemindMessage>(entity)));
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
        await AuditLogService.StoreItemAsync(logItem);

        remind.RemindMessageId = messageId.ToString();
        await repository.CommitAsync();
    }
}
