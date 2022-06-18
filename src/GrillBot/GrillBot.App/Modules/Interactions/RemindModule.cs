using Discord.Interactions;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Modules.Implementations.Reminder;
using GrillBot.App.Services.Reminder;
using GrillBot.Common;
using GrillBot.Common.Helpers;

namespace GrillBot.App.Modules.Interactions;

[Group("remind", "Připomenutí k určitému datu")]
[RequireUserPerms]
public class RemindModule : Infrastructure.InteractionsModuleBase
{
    private RemindService RemindService { get; }

    public RemindModule(RemindService remindService)
    {
        RemindService = remindService;
    }

    [SlashCommand("create", "Vytvoření připomenutí k určitému datu.")]
    public async Task CreateAsync(
        [Summary("komu", "Označení uživatele, který si přeje dostat upozornění")]
        IUser who,
        [Summary("kdy", "Datum a čas události. Musí být v budoucnosti.")]
        DateTime at,
        [Summary("zprava", "Zpráva, která se zašle uživateli.")]
        string message,
        [Summary("tajne", "Toto upozornění má být skryto před zraky ostatních.")] [Choice("Ano", "true")] [Choice("Ne", "false")]
        bool secret = false
    )
    {
        try
        {
            await DeferAsync(secret);

            var originalMessage = await Context.Interaction.GetOriginalResponseAsync();
            var remindId = await RemindService.CreateRemindAsync(Context.User, who, at, message, originalMessage.Id);

            var buttons = secret ? null : new ComponentBuilder().WithButton(customId: $"remind_copy:{remindId}", emote: Emojis.PersonRisingHand).Build();
            var msg = $"Připomenutí bylo vytvořeno.{(secret ? "" : $"Pokud si někdo přeje dostat toto upozornění také, tak ať klikne na tlačítko {Emojis.PersonRisingHand}")}";
            await SetResponseAsync(msg, components: buttons, secret: secret);
        }
        catch (ValidationException ex)
        {
            await SetResponseAsync(ex.Message, secret: secret);
        }
    }

    [SlashCommand("cancel", "Předčasné zrušení připomenutí.")]
    public async Task CancelRemindAsync(
        [Summary("ident", "Identifikace připomenutí")] [Autocomplete(typeof(RemindAutoCompleteHandler))]
        long id,
        [Summary("upozornit", "Zda se má cílový uživatel předčasně upozornit.")] [Choice("Ano", "true")] [Choice("Ne", "false")]
        bool notify = false
    )
    {
        try
        {
            await RemindService.CancelRemindAsync(id, Context.User, notify);
            await SetResponseAsync($"Upozornění bylo úspěšně zrušeno{(notify ? " a cílový uživatel byl upozorněn" : "")}.");
        }
        catch (ValidationException ex)
        {
            await SetResponseAsync(ex.Message);
        }
    }

    [SlashCommand("list", "Seznam aktuálně čekajících připomenutí.")]
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
        bool canDefer = true;

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
