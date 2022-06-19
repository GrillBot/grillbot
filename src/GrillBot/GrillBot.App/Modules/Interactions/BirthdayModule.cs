using Discord.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Services.Birthday;

namespace GrillBot.App.Modules.Interactions;

[Group("narozeniny", "Narozeniny")]
public class BirthdayModule : InteractionsModuleBase
{
    private BirthdayService BirthdayService { get; }
    private IConfiguration Configuration { get; }

    public BirthdayModule(BirthdayService birthdayService, IConfiguration configuration)
    {
        BirthdayService = birthdayService;
        Configuration = configuration;
    }

    [SlashCommand("dnes", "Zjištění, kdo má dnes narozeniny.")]
    public async Task TodayBirthdayAsync()
    {
        var users = await BirthdayService.GetTodayBirthdaysAsync();
        await SetResponseAsync(BirthdayHelper.Format(users, Configuration));
    }

    [SlashCommand("pridat", "Přidání tvého data narození.")]
    public async Task AddAsync(
        [Summary("kdy", "Datum, kdy máš narozeniny. (Formát: yyyy-mm-dd, pokud nechceš rok, tak zadej jako rok 0001).")]
        DateTime when
    )
    {
        await BirthdayService.AddBirthdayAsync(Context.User, when);

        if (Context.Guild.CurrentUser.GuildPermissions.ManageMessages)
        {
            await ReplyAsync($"{Context.User.Mention} Datum narození bylo úspěšně uloženo.", allowedMentions: new AllowedMentions() { AllowedTypes = AllowedMentionTypes.Users });
            await DeleteOriginalResponseAsync();
        }
        else
        {
            await SetResponseAsync("Datum narození bylo úspěšně uloženo.");
        }
    }

    [SlashCommand("smazat", "Smazání data narození.")]
    public async Task RemoveAsync()
    {
        await BirthdayService.RemoveBirthdayAsync(Context.User);
        await SetResponseAsync("Datum narození bylo úspěšně odebráno.");
    }

    [SlashCommand("mam", "Dotaz, jestli mám uložené narozeniny?")]
    public async Task HaveAsync()
    {
        if (await BirthdayService.HaveBirthdayAsync(Context.User))
            await SetResponseAsync("Ano. Máš uložené narozeniny.");
        else
            await SetResponseAsync("Ne. Nemáš uložené narozeniny.");
    }
}
