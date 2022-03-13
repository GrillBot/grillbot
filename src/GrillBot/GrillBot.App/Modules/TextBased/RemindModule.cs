using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;
using GrillBot.App.Modules.Implementations.Reminder;
using GrillBot.App.Services.Reminder;

namespace GrillBot.App.Modules.TextBased;

[Group("remind")]
[Name("Připomínání")]
[Infrastructure.Preconditions.TextBased.RequireUserPerms]
public class RemindModule : Infrastructure.ModuleBase
{
    private RemindService RemindService { get; }

    public RemindModule(RemindService remindService)
    {
        RemindService = remindService;
    }

    [Command("")]
    [TextCommandDeprecated(AlternativeCommand = "/remind")]
    public Task CreateAsync([Name("komu")] IUser _, [Name("kdy")] DateTime __, [Remainder][Name("zprava")] string ___) => Task.CompletedTask;

    [Command("")]
    [Summary("Získá seznam čekajících upozornění pro daného uživatele.")]
    [RequireBotPermission(ChannelPermission.AddReactions, ErrorMessage = "Nemohu provést tento příkaz, protože nemám v tomto kanálu oprávnění přidávat reakce.")]
    public async Task GetRemindListAsync()
    {
        var data = await RemindService.GetRemindersAsync(Context.User, 0);

        var embed = await new EmbedBuilder()
            .WithRemindListAsync(data, Context.Client, Context.User, Context.User, 0);

        var message = await ReplyAsync(embed: embed.Build());
        if (data.Count >= EmbedBuilder.MaxFieldCount)
            await message.AddReactionsAsync(Emojis.PaginationEmojis);
    }

    [Command("cancel")]
    [Alias("zrusit")]
    [Summary("Zruší upozornění. Případně může upozornit předčasně.")]
    public async Task CancelReminderAsync(long id, [Name("upozornit")] bool notify = false)
    {
        try
        {
            await RemindService.CancelRemindAsync(id, Context.User, notify);
            await ReplyAsync($"Upozornění bylo úspěšně zrušeno{(notify ? " a cílový uživatel byl upozorněn" : "")}.");
        }
        catch (ValidationException ex)
        {
            await ReplyAsync(ex.Message);
        }
    }
}
