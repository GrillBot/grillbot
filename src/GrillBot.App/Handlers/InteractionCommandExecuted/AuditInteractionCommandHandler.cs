using Discord.Interactions;
using GrillBot.App.Managers;
using GrillBot.App.Services.Discord;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Handlers.InteractionCommandExecuted;

public class AuditInteractionCommandHandler : IInteractionCommandExecutedEvent
{
    private AuditLogWriteManager AuditLogWriteManager { get; }

    public AuditInteractionCommandHandler(AuditLogWriteManager auditLogWriteManager)
    {
        AuditLogWriteManager = auditLogWriteManager;
    }

    public async Task ProcessAsync(ICommandInfo commandInfo, IInteractionContext context, IResult result)
    {
        if (!Init(result, context, out var duration)) return;

        var data = GrillBot.Data.Models.AuditLog.InteractionCommandExecuted.Create(context.Interaction, commandInfo, result, duration);
        var item = new AuditLogDataWrapper(AuditLogItemType.InteractionCommand, data, context.Guild, context.Channel as IGuildChannel, context.User);
        await AuditLogWriteManager.StoreAsync(item);
    }

    private static bool Init(IResult result, IInteractionContext context, out int duration)
    {
        duration = CommandsPerformanceCounter.TaskExists(context) ? CommandsPerformanceCounter.TaskFinished(context) : 0;
        return result.Error != InteractionCommandError.UnknownCommand;
    }
}
