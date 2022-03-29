using Discord.Interactions;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog.Events;

public class ExecutedInteractionCommandEvent : AuditEventBase
{
    private ICommandInfo Command { get; }
    private IInteractionContext Context { get; }
    private IResult Result { get; }

    public ExecutedInteractionCommandEvent(AuditLogService auditLogService, ICommandInfo command, IInteractionContext context,
        IResult result) : base(auditLogService)
    {
        Command = command;
        Context = context;
        Result = result;
    }

    public override Task<bool> CanProcessAsync()
        => Task.FromResult(true);

    public override async Task ProcessAsync()
    {
        var data = InteractionCommandExecuted.Create(Context.Interaction, Command, Result);
        var item = new AuditLogDataWrapper(AuditLogItemType.InteractionCommand, data, Context.Guild, Context.Channel, Context.User);
        await AuditLogService.StoreItemAsync(item);
    }
}
