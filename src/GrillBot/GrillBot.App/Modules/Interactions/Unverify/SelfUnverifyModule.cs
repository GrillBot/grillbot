using Discord.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Services.Unverify;
using GrillBot.Common;

namespace GrillBot.App.Modules.Interactions.Unverify;

[RequireUserPerms]
public class SelfUnverifyModule : InteractionsModuleBase
{
    private UnverifyService UnverifyService { get; }

    public SelfUnverifyModule(UnverifyService unverifyService, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        UnverifyService = unverifyService;
    }

    [SlashCommand("selfunverify", "Temporarily remove access to yourself on the server.")]
    [RequireBotPermission(GuildPermission.AddReactions | GuildPermission.ManageRoles)]
    public async Task SelfUnverifyAsync(
        [Summary("end", "End date and time, or duration of access removal.")]
        DateTime end,
        [Summary("keepables", "A list of allowable accesses. Separate with a comma, space, or semicolon.")]
        string keepables = null
    )
    {
        keepables ??= "";

        try
        {
            end = end.AddMinutes(1); // Strinct checks are only in unverify.
            var toKeep = keepables.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(o => o.ToUpper()).ToList();
            var guildUser = Context.User as SocketGuildUser ?? Context.Guild.GetUser(Context.User.Id);
            var result = await UnverifyService.SetUnverifyAsync(guildUser, end, null, Context.Guild, guildUser, true, toKeep, null, false, Locale);
            await SetResponseAsync(result);
        }
        catch (Exception ex)
        {
            if (ex is ValidationException)
            {
                await SetResponseAsync(ex.Message);
            }
            else
            {
                await SetResponseAsync(Texts["Unverify/SelfUnverify/GenericError", Locale]);
                throw;
            }
        }
    }
}
