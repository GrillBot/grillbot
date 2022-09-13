using Discord.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Services.Birthday;

namespace GrillBot.App.Modules.Interactions;

[Group("birthdays", "Birthdays")]
public class BirthdayModule : InteractionsModuleBase
{
    private BirthdayService BirthdayService { get; }
    private IConfiguration Configuration { get; }

    public BirthdayModule(BirthdayService birthdayService, IConfiguration configuration)
    {
        BirthdayService = birthdayService;
        Configuration = configuration;
    }

    [SlashCommand("today", "Finding out who's birthday is today.")]
    public async Task TodayBirthdayAsync()
    {
        var users = await BirthdayService.GetTodayBirthdaysAsync();
        await SetResponseAsync(BirthdayHelper.Format(users, Configuration));
    }

    [SlashCommand("add", "Adding your date of birth.")]
    public async Task AddAsync(
        [Summary("when", "The date of your birthday. (Format: yyyy-mm-dd, if you don't want a year, enter 0001 as the year).")]
        DateTime when
    )
    {
        await BirthdayService.AddBirthdayAsync(Context.User, when);

        if (Context.Guild.CurrentUser.GuildPermissions.ManageMessages)
        {
            await ReplyAsync($"{Context.User.Mention} Datum narození bylo úspěšně uloženo.", allowedMentions: new AllowedMentions { AllowedTypes = AllowedMentionTypes.Users });
            await DeleteOriginalResponseAsync();
        }
        else
        {
            await SetResponseAsync("Datum narození bylo úspěšně uloženo.");
        }
    }

    [SlashCommand("remove", "Delete date of birth.")]
    public async Task RemoveAsync()
    {
        await BirthdayService.RemoveBirthdayAsync(Context.User);
        await SetResponseAsync("Datum narození bylo úspěšně odebráno.");
    }

    [SlashCommand("have", "Ask if I have my birthday saved?")]
    public async Task HaveAsync()
    {
        if (await BirthdayService.HaveBirthdayAsync(Context.User))
            await SetResponseAsync("Ano. Máš uložené narozeniny.");
        else
            await SetResponseAsync("Ne. Nemáš uložené narozeniny.");
    }
}
