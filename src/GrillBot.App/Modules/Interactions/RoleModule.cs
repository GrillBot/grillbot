﻿using Discord.Interactions;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.Preconditions.Interactions;

namespace GrillBot.App.Modules.Interactions;

[Group("role", "Roles management")]
[RequireBotPermission(GuildPermission.ManageRoles)]
[RequireUserPerms]
public class RoleModule : InteractionsModuleBase
{
    public RoleModule(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [SlashCommand("list", "List of roles")]
    public async Task GetListAsync(
        [Choice("Members", "members")] [Choice("Position", "position")]
        string sortBy = "position"
    )
    {
        using var command = GetCommand<Actions.Commands.RolesReader>();

        var result = await command.Command.ProcessListAsync(sortBy);
        await SetResponseAsync(embed: result);
    }

    [SlashCommand("get", "Role detail")]
    public async Task GetDetailAsync(IRole role)
    {
        using var command = GetCommand<Actions.Commands.RolesReader>();

        var result = await command.Command.ProcessDetailAsync(role);
        await SetResponseAsync(embed: result);
    }
}
