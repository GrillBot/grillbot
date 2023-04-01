using Discord.Interactions;
using GrillBot.App.Actions.Api.V2;
using GrillBot.App.Actions.Commands.Birthday;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.Preconditions.Interactions;

namespace GrillBot.App.Modules.Interactions;

[Group("birthdays", "Birthdays")]
[RequireUserPerms]
public class BirthdayModule : InteractionsModuleBase
{
    public BirthdayModule(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [SlashCommand("today", "Finding out who's birthday is today.")]
    public async Task TodayBirthdayAsync()
    {
        using var command = GetActionAsCommand<GetTodayBirthdayInfo>();
        var result = await command.Command.ProcessAsync();

        await SetResponseAsync(result.Message);
    }

    [SlashCommand("add", "Adding your date of birth.")]
    public async Task AddAsync(
        [Summary("when", "The date of your birthday. (Format: yyyy-mm-dd, if you don't want a year, enter 0001 as the year).")]
        DateTime when
    )
    {
        using var command = GetCommand<AddBirthday>();
        await command.Command.ProcessAsync(when);

        if (Context.Guild.CurrentUser.GuildPermissions.ManageMessages)
        {
            await ReplyAsync(Context.User.Mention + " " + GetText(nameof(AddAsync), "Success"),
                allowedMentions: new AllowedMentions { AllowedTypes = AllowedMentionTypes.Users });
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
        using var command = GetCommand<RemoveBirthday>();
        await command.Command.ProcessAsync();
        await SetResponseAsync(GetText(nameof(RemoveAsync), "Success"));
    }

    [SlashCommand("have", "Ask if I have my birthday saved?")]
    public async Task HaveAsync()
    {
        using var command = GetCommand<HaveBirthday>();

        var result = await command.Command.ProcessAsync();
        var localeKey = result ? "Yes" : "No";
        await SetResponseAsync(GetText(nameof(HaveAsync), localeKey));
    }
}
