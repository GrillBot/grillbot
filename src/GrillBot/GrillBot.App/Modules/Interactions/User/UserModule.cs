using Discord.Interactions;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Modules.Implementations.User;

namespace GrillBot.App.Modules.Interactions.User;

[Group("user", "User management")]
[RequireUserPerms(GuildPermission.ViewAuditLog)]
[DefaultMemberPermissions(GuildPermission.ViewAuditLog | GuildPermission.UseApplicationCommands)]
public class UserModule : Infrastructure.Commands.InteractionsModuleBase
{
    public UserModule(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [SlashCommand("access", "View a list of user permissions.")]
    [DeferConfiguration(RequireEphemeral = true)]
    public async Task GetAccessListAsync([Summary("user", "User identification")] IGuildUser user)
    {
        using var command = GetCommand<Actions.Commands.UserAccessList>();

        var (embed, paginationComponents) = await command.Command.ProcessAsync(user, 0);
        await SetResponseAsync(embed: embed, components: paginationComponents, secret: true);
    }

    [UserCommand("Seznam oprávnění")]
    [DeferConfiguration(RequireEphemeral = true)]
    public async Task GetAccessListFromContextMenuAsync(IGuildUser user)
    {
        await GetAccessListAsync(user);
    }

    [RequireSameUserAsAuthor]
    [ComponentInteraction("user_access:*", ignoreGroupNames: true)]
    public async Task HandleAccessListPaginationAsync(int page)
    {
        var handler = new UserAccessListHandler(ServiceProvider, page);
        await handler.ProcessAsync(Context);
    }

    [SlashCommand("info", "Information about user")]
    public async Task UserInfoAsync(IGuildUser user)
    {
        using var command = GetCommand<Actions.Commands.UserInfo>();

        var result = await command.Command.ProcessAsync(user);
        await SetResponseAsync(embed: result);
    }
}
