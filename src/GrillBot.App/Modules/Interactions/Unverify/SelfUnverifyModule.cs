using Discord.Interactions;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.Preconditions.Interactions;

namespace GrillBot.App.Modules.Interactions.Unverify;

[RequireUserPerms]
public class SelfUnverifyModule : InteractionsModuleBase
{
    public SelfUnverifyModule(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [SlashCommand("selfunverify", "Temporarily remove access to yourself on the server.")]
    [RequireBotPermission(GuildPermission.AddReactions | GuildPermission.ManageRoles)]
    public async Task SelfUnverifyAsync(
        [Summary("end", "End date and time, or duration of access removal.")]
        DateTime end,
        [Summary("keepables", "A list of allowable accesses. Separate with a comma, space, or semicolon.")]
        string? keepables = null
    )
    {
        keepables ??= "";

        using var command = GetCommand<Actions.Commands.Unverify.SetUnverify>();

        try
        {
            end = end.AddMinutes(1); // Strinct checks are only in unverify.
            var toKeep = keepables.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(o => o.ToUpper()).ToList();
            var guildUser = Context.User as IGuildUser ?? Context.Guild.GetUser(Context.User.Id);

            var result = await command.Command.ProcessAsync(guildUser, end, null, true, toKeep, false);
            await SetResponseAsync(result);
        }
        catch (ValidationException ex)
        {
            await SetResponseAsync(ex.Message);
        }
    }
}
