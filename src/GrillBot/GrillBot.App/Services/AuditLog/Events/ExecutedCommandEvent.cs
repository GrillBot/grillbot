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

    public ExecutedCommandEvent(AuditLogService auditLogService, CommandInfo command, ICommandContext context,
        IResult result) : base(auditLogService)
    {
        Command = command;
        Context = context;
        Result = result;
    }

    public override Task<bool> CanProcessAsync()
    {
        // Do not log deprecated text commands.
        if (Result?.IsSuccess == false && Result.Error == CommandError.UnmetPrecondition && Result.ErrorReason.StartsWith(TextCommandDeprecatedAttribute.Prefix))
            return Task.FromResult(false);

        return Task.FromResult(true);
    }

    public override async Task ProcessAsync()
    {
        var data = new CommandExecution(Command, Context.Message, Result);
        var jsonData = JsonConvert.SerializeObject(data, AuditLogService.JsonSerializerSettings);
        await AuditLogService.StoreItemAsync(AuditLogItemType.Command, Context.Guild, Context.Channel, Context.User, jsonData, null, null, null);
    }
}
