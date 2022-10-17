using Discord.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Modules.Implementations.Unverify;
using GrillBot.Data.Exceptions;

namespace GrillBot.App.Modules.Interactions.Unverify;

[Group("unverify", "Unverify management")]
[DefaultMemberPermissions(GuildPermission.UseApplicationCommands | GuildPermission.ManageRoles)]
[RequireUserPerms(GuildPermission.ManageRoles)]
public class UnverifyModule : InteractionsModuleBase
{
    public UnverifyModule(IServiceProvider serviceProvider) : base(null, serviceProvider)
    {
    }

    [SlashCommand("list", "List of all current unverifies")]
    public async Task UnverifyListAsync()
    {
        using var command = GetCommand<Actions.Commands.Unverify.UnverifyList>();

        try
        {
            var (embed, paginationComponent) = await command.Command.ProcessAsync(0);
            await SetResponseAsync(embed: embed, components: paginationComponent);
        }
        catch (NotFoundException ex)
        {
            await SetResponseAsync(ex.Message);
        }
    }

    [RequireSameUserAsAuthor]
    [ComponentInteraction("unverify:*", ignoreGroupNames: true)]
    public async Task HandleUnverifyListPaginationAsync(int page)
    {
        var handler = new UnverifyListPaginationHandler(page, ServiceProvider);
        await handler.ProcessAsync(Context);
    }
}
