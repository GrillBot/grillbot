using Discord.Interactions;
using GrillBot.App.Helpers;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Modules.Implementations.Reminder;
using GrillBot.Common;
using GrillBot.Core.Exceptions;

namespace GrillBot.App.Modules.Interactions;

[Group("remind", "A reminder for a specific date")]
[RequireUserPerms]
public class RemindModule : InteractionsModuleBase
{
    public RemindModule(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [SlashCommand("create", "Create a reminder for a specific date.")]
    [DeferConfiguration(SuppressAuto = true)]
    public async Task CreateAsync(
        [Summary("who", "Designation of the user who wishes to receive notifications")]
        IUser who,
        [Summary("when", "Reminder date and time. It must be in the future.")]
        DateTime at,
        [Summary("message", "The message that will be sent to the user.")]
        string message,
        [Summary("secret", "This notification should be hidden from others.")] [Choice("Yes", "true")] [Choice("No", "false")]
        bool secret = false
    )
    {
        await DeferAsync(secret);

        var originalMessage = await Context.Interaction.GetOriginalResponseAsync();
        try
        {
            using var command = GetCommand<Actions.Commands.Reminder.CreateRemind>();
            var remindId = await command.Command.ProcessAsync(Context.User, who, at, message, originalMessage.Id);

            var buttons = secret ? null : RemindHelper.CreateCopyButton(remindId);
            var msg = GetText(nameof(CreateAsync), "Success") + (secret ? "" : " " + GetText(nameof(CreateAsync), "CopyMessage").FormatWith(Emojis.PersonRisingHand.ToString()));
            await SetResponseAsync(msg, components: buttons, secret: secret);
        }
        catch (ValidationException ex)
        {
            await SetResponseAsync(ex.Message, secret: secret);
        }
    }

    [SlashCommand("cancel", "Early cancellation of reminders.")]
    public async Task CancelRemindAsync(
        [Summary("ident", "Reminder identification")] [Autocomplete(typeof(RemindAutoCompleteHandler))]
        long id,
        [Summary("notify", "Whether to notify the target user early.")] [Choice("Yes", "true")] [Choice("No", "false")]
        bool notify = false
    )
    {
        using var action = GetActionAsCommand<Actions.Api.V1.Reminder.FinishRemind>();
        await action.Command.ProcessAsync(id, notify, false);

        if (action.Command.IsGone || !action.Command.IsAuthorized)
            await SetResponseAsync(action.Command.ErrorMessage);
        else
            await SetResponseAsync(GetText(nameof(CancelRemindAsync), notify ? "CancelledWithNotify" : "Cancelled"));
    }

    [SlashCommand("list", "List of currently pending reminders.")]
    public async Task RemindListAsync()
    {
        using var command = GetCommand<Actions.Commands.Reminder.GetReminderList>();
        var (embed, paginationComponent) = await command.Command.ProcessAsync(0);
        
        await SetResponseAsync(embed: embed, components: paginationComponent);
    }

    [RequireSameUserAsAuthor]
    [ComponentInteraction("remind:*", ignoreGroupNames: true)]
    public async Task HandleRemindListPaginationAsync(int page)
    {
        var handler = new RemindPaginationHandler(page, ServiceProvider);
        await handler.ProcessAsync(Context);
    }

    [ComponentInteraction("remind_copy:*", ignoreGroupNames: true)]
    public async Task HandleRemindCopyAsync(long remindId)
    {
        var canDefer = true;

        using var command = GetCommand<Actions.Commands.Reminder.CopyRemind>();
        try
        {
            await command.Command.ProcessAsync(remindId);
        }
        catch (ValidationException ex)
        {
            await RespondAsync(ex.Message, ephemeral: true);
            canDefer = false;
        }
        catch (NotFoundException ex)
        {
            await RespondAsync(ex.Message, ephemeral: true);
            canDefer = false;
        }

        if (canDefer)
            await DeferAsync();
    }

    [ComponentInteraction("remind_postpone:*", ignoreGroupNames: true)]
    [AllowDms]
    public async Task HandleRemindPostponeAsync(int hours)
    {
        var handler = new RemindPostponeHandler(hours, ServiceProvider);
        await handler.ProcessAsync(Context);
    }
}
