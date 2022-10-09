using System.Diagnostics.CodeAnalysis;
using Discord.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
[Group("permissions", "Manage channel permissions.")]
[ExcludeFromCodeCoverage]
public class PermissionsModule : InteractionsModuleBase
{
    [Group("remove", "Permissions removal processing")]
    [RequireBotPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
    public class PermissionsRemoveSubModule : InteractionsModuleBase
    {
        public PermissionsRemoveSubModule(IServiceProvider serviceProvider) : base(null, serviceProvider)
        {
        }

        [SlashCommand("all", "Remove all user permissions from channel.")]
        public async Task ClearPermissionsFromChannelAsync(IGuildChannel channel, IGuildUser excludedUser = null)
        {
            var excludedUsers = excludedUser != null ? new[] { excludedUser } : Array.Empty<IGuildUser>();

            using var command = GetCommand<Actions.Commands.PermissionsCleaner>();
            command.Command.OnProgress = async progressBar => await SetResponseAsync(progressBar, suppressFollowUp: true);
            await command.Command.ClearAllPermissionsAsync(channel, excludedUsers);
        }
    }
}
