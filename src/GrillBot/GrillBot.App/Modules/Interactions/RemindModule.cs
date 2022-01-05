using Discord;
using Discord.Interactions;
using GrillBot.App.Services.Reminder;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace GrillBot.App.Modules.Interactions;

public class RemindModule : Infrastructure.InteractionsModuleBase
{
    private RemindService RemindService { get; }

    public RemindModule(RemindService remindService)
    {
        RemindService = remindService;
    }

    [SlashCommand("remind", "Vytvoření připomenutí k určitému datu.")]
    public async Task CreateAsync(
        [Summary("komu", "Označení uživatele, který si přeje dostat upozornění")]
        IUser who,
        [Summary("kdy", "Datum a čas události. Musí být v budoucnosti.")]
        DateTime at,
        [Summary("zprava", "Zpráva, která se zašle uživateli.")]
        string message
    )
    {
        try
        {
            var originalMessage = await Context.Interaction.GetOriginalResponseAsync();
            await RemindService.CreateRemindAsync(Context.User, who, at, message, originalMessage);
            await SetResponseAsync("Upozornění vytvořeno.");
        }
        catch (ValidationException ex)
        {
            await SetResponseAsync(ex.Message);
        }
    }
}
