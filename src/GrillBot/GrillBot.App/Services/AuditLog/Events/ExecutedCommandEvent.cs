using Discord.Commands;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog.Events;

public class ExecutedCommandEvent : AuditEventBase
{
    private CommandInfo Command { get; }
    private ICommandContext Context { get; }
    private IResult Result { get; }

    public ExecutedCommandEvent(AuditLogService auditLogService, CommandInfo command, ICommandContext context,
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
        var data = new CommandExecution(Command, Context.Message, Result);
        var jsonData = JsonConvert.SerializeObject(data, AuditLogService.JsonSerializerSettings);
        await AuditLogService.StoreItemAsync(AuditLogItemType.Command, Context.Guild, Context.Channel, Context.User, jsonData, null, null, null);
    }
}
