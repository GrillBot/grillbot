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
        var jsonData = JsonConvert.SerializeObject(data, AuditLogService.JsonSerializerSettings);
        await AuditLogService.StoreItemAsync(AuditLogItemType.InteractionCommand, Context.Guild, Context.Channel, Context.User, jsonData, null, null, null);
    }
}
