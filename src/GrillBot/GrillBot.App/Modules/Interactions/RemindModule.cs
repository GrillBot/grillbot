using Discord.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Modules.Implementations.Reminder;
using GrillBot.App.Services.Reminder;
using GrillBot.Common;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers;

namespace GrillBot.App.Modules.Interactions;

[Group("remind", "A reminder for a specific date")]
[RequireUserPerms]
public class RemindModule : InteractionsModuleBase
{
    private RemindService RemindService { get; }

    public RemindModule(RemindService remindService, LocalizationManager localization) : base(localization)
    {
        RemindService = remindService;
    }

    [SlashCommand("create", "Create a reminder for a specific date.")]
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
        try
        {
            await DeferAsync(secret);

            var originalMessage = await Context.Interaction.GetOriginalResponseAsync();
            var remindId = await RemindService.CreateRemindAsync(Context.User, who, at, message, originalMessage.Id);

            var buttons = secret ? null : new ComponentBuilder().WithButton(customId: $"remind_copy:{remindId}", emote: Emojis.PersonRisingHand).Build();
            var msg = GetLocale(nameof(CreateAsync), "Success") + (secret ? "" : " " + GetLocale(nameof(CreateAsync), "CopyMessage").FormatWith(Emojis.PersonRisingHand.ToString()));
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
        try
        {
            await RemindService.CancelRemindAsync(id, Context.User, notify);
            await SetResponseAsync(GetLocale(nameof(CancelRemindAsync), notify ? "CancelledWithNotify" : "Cancelled"));
        }
        catch (ValidationException ex)
        {
            await SetResponseAsync(ex.Message);
        }
    }

    [SlashCommand("list", "List of currently pending reminders.")]
    public async Task RemindListAsync()
    {
        var data = await RemindService.GetRemindersAsync(Context.User, 0);
        var remindsCount = await RemindService.GetRemindersCountAsync(Context.User);
        var pagesCount = (int)Math.Ceiling(remindsCount / (double)EmbedBuilder.MaxFieldCount);

        var embed = await new EmbedBuilder()
            .WithRemindListAsync(data, Context.Client, Context.User, Context.User, 0);

        var components = ComponentsHelper.CreatePaginationComponents(0, pagesCount, "remind");
        await SetResponseAsync(embed: embed.Build(), components: components);
    }

    [RequireSameUserAsAuthor]
    [ComponentInteraction("remind:*", ignoreGroupNames: true)]
    public async Task HandleRemindListPaginationAsync(int page)
    {
        var handler = new RemindPaginationHandler(RemindService, Context.Client, page);
        await handler.ProcessAsync(Context);
    }

    [ComponentInteraction("remind_copy:*", ignoreGroupNames: true)]
    public async Task HandleRemindCopyAsync(long remindId)
    {
        var canDefer = true;

        try
        {
            await RemindService.CopyAsync(remindId, Context.User);
        }
        catch (ValidationException ex)
        {
            await RespondAsync(ex.Message, ephemeral: true);
            canDefer = false;
        }
        catch (InvalidOperationException ex)
        {
            await RespondAsync(ex.Message, ephemeral: true);
            await ((SocketMessageComponent)Context.Interaction).UpdateAsync(o => o.Components = null);
            canDefer = false;
        }

        if (canDefer)
            await DeferAsync();
    }
}
