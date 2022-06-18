using Discord.Interactions;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog.Events;

public class ExecutedInteractionCommandEvent : AuditEventBase
{
    private ICommandInfo Command { get; }
    private IInteractionContext Context { get; }
    private IResult Result { get; }
    private int Duration { get; }

    public ExecutedInteractionCommandEvent(AuditLogService auditLogService, AuditLogWriter auditLogWriter, ICommandInfo command, IInteractionContext context,
        IResult result, int duration) : base(auditLogService, auditLogWriter)
    {
        Command = command;
        Context = context;
        Result = result;
        Duration = duration;
    }

    public override Task<bool> CanProcessAsync()
        => Task.FromResult(true);

    public override async Task ProcessAsync()
    {
        var data = InteractionCommandExecuted.Create(Context.Interaction, Command, Result, Duration);
        var item = new AuditLogDataWrapper(AuditLogItemType.InteractionCommand, data, Context.Guild, Context.Channel as IGuildChannel, Context.User);
        await AuditLogWriter.StoreAsync(item);
    }
}
