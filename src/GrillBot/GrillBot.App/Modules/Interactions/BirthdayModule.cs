using Discord.Interactions;
using GrillBot.App.Actions.Api.V2;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Services.Birthday;
using Microsoft.Extensions.DependencyInjection;
using GrillBot.Common.Managers.Localization;

namespace GrillBot.App.Modules.Interactions;

[Group("birthdays", "Birthdays")]
public class BirthdayModule : InteractionsModuleBase
{
    private BirthdayService BirthdayService { get; }
    private IServiceProvider ServiceProvider { get; }
    private IConfiguration Configuration { get; }

    public BirthdayModule(BirthdayService birthdayService, IConfiguration configuration, ITextsManager texts, IServiceProvider serviceProvider) : base(texts)
    {
        BirthdayService = birthdayService;
        ServiceProvider = serviceProvider;
        Configuration = configuration;
    }

    [SlashCommand("today", "Finding out who's birthday is today.")]
    public async Task TodayBirthdayAsync()
    {
        using var scope = ServiceProvider.CreateScope();

        var action = scope.ServiceProvider.GetRequiredService<GetTodayBirthdayInfo>();
        var result = await action.ProcessAsync();

        await SetResponseAsync(result);
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
            await ReplyAsync(Context.User.Mention + " " + GetText(nameof(AddAsync), "Success"), allowedMentions: new AllowedMentions { AllowedTypes = AllowedMentionTypes.Users });
            await DeleteOriginalResponseAsync();
        }
        else
        {
            await SetResponseAsync(GetText(nameof(AddAsync), "Success"));
        }
    }

    [SlashCommand("remove", "Delete date of birth.")]
    public async Task RemoveAsync()
    {
        await BirthdayService.RemoveBirthdayAsync(Context.User);
        await SetResponseAsync(GetText(nameof(RemoveAsync), "Success"));
    }

    [SlashCommand("have", "Ask if I have my birthday saved?")]
    public async Task HaveAsync()
    {
        var localeKey = await BirthdayService.HaveBirthdayAsync(Context.User) ? "Yes" : "No";
        await SetResponseAsync(GetText(nameof(HaveAsync), localeKey));
    }
}
