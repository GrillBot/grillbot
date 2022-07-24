using Discord.Interactions;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Modules.Implementations.User;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;

namespace GrillBot.App.Modules.Interactions.User;

[Group("user", "Správa uživatelů")]
[RequireUserPerms(GuildPermission.ViewAuditLog)]
[DefaultMemberPermissions(GuildPermission.ViewAuditLog | GuildPermission.UseApplicationCommands)]
public class UserModule : Infrastructure.Commands.InteractionsModuleBase
{
    [SlashCommand("access", "Zobrazení seznamu oprávnění uživatele.")]
    public async Task GetAccessListAsync(
        [Summary("uzivatel", "Identifikace uživatele")]
        IGuildUser user,
        [Summary("tajne", "Zobrazit výsledek soukromě?")] [Choice("Ano", "true")] [Choice("Ne", "false")]
        bool secret = false
    )
    {
        var visibleChannels = await Context.Guild.GetAvailableChannelsAsync(user);
        visibleChannels = visibleChannels.FindAll(o => o is not ICategoryChannel);

        var embed = new EmbedBuilder()
            .WithUserAccessList(visibleChannels, user, Context.User, Context.Guild, 0, out var pagesCount)
            .Build();

        var components = ComponentsHelper.CreatePaginationComponents(0, pagesCount, "user_access");
        await SetResponseAsync(embed: embed, components: components, secret: secret);
    }

    [UserCommand("Seznam oprávnění")]
    [SuppressDefer]
    [DefaultMemberPermissions(GuildPermission.ManageRoles)]
    public async Task GetAccessListFromContextMenuAsync(IGuildUser user)
    {
        await GetAccessListAsync(user, true);
    }

    [RequireSameUserAsAuthor]
    [ComponentInteraction("user_access:*", ignoreGroupNames: true)]
    [DefaultMemberPermissions(GuildPermission.ManageRoles)]
    public async Task HandleAccessListPaginationAsync(int page)
    {
        var handler = new UserAccessListHandler(Context.Client, page);
        await handler.ProcessAsync(Context);
    }
}
