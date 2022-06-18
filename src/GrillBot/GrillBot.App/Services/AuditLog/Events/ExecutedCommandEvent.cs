using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog.Events;

public class ExecutedCommandEvent : AuditEventBase
{
    private CommandInfo Command { get; }
    private ICommandContext Context { get; }
    private IResult Result { get; }
    private int Duration { get; }

    public ExecutedCommandEvent(AuditLogService auditLogService, AuditLogWriter auditLogWriter, CommandInfo command, ICommandContext context,
        IResult result, int duration) : base(auditLogService, auditLogWriter)
    {
        Command = command;
        Context = context;
        Result = result;
        Duration = duration;
    }

    public override Task<bool> CanProcessAsync()
    {
        // Do not log deprecated text commands.
        return Task.FromResult(Result is not { IsSuccess: false, Error: CommandError.UnmetPrecondition } || !Result.ErrorReason.StartsWith(TextCommandDeprecatedAttribute.Prefix));
    }

    public override async Task ProcessAsync()
    {
        var data = new CommandExecution(Command, Context.Message, Result, Duration);
        var item = new AuditLogDataWrapper(AuditLogItemType.Command, data, Context.Guild, Context.Channel as IGuildChannel, Context.User);
        await AuditLogWriter.StoreAsync(item);
    }
}
