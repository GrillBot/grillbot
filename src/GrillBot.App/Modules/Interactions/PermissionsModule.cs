using Discord.Interactions;
using GrillBot.App.Actions.Commands.Permissions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;

namespace GrillBot.App.Modules.Interactions;

[Group("permissions", "Manage channel permissions.")]
[RequireUserPerms]
public class PermissionsModule : InteractionsModuleBase
{
    public PermissionsModule(IServiceProvider provider) : base(provider)
    {
    }

    [Group("remove", "Permissions removal processing")]
    [RequireBotPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
    public class PermissionsRemoveSubModule : InteractionsModuleBase
    {
        public PermissionsRemoveSubModule(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        [SlashCommand("all", "Remove all user permissions from channel.")]
        public async Task ClearPermissionsFromChannelAsync(IGuildChannel channel, IEnumerable<IUser>? excludedUsers = null)
        {
            var users = (excludedUsers ?? new List<IUser>()).Select(o => o as IGuildUser ?? Context.Guild.GetUser(o.Id)).Where(o => o != null).ToList();

            using var command = GetCommand<PermissionsCleaner>();
            command.Command.OnProgress = async progressBar => await SetResponseAsync(progressBar, suppressFollowUp: true);
            await command.Command.ClearAllPermissionsAsync(channel, users);
        }
    }

    [Group("set", "Permissions set processing")]
    [RequireBotPermission(ChannelPermission.ManageChannels | ChannelPermission.ManageRoles)]
    public class PermissionsSetSubModule : InteractionsModuleBase
    {
        public PermissionsSetSubModule(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        [SlashCommand("category", "Set permissions with a role for all channels in a category.")]
        public async Task SetPermsToCategoryChannelsAsync(ICategoryChannel category, IRole role, [Choice("Allow", "true")] [Choice("Deny", "false")] bool viewChannel)
        {
            using var command = GetCommand<PermissionSetter>();
            command.Command.OnProgress = async progressBar => await SetResponseAsync(progressBar, suppressFollowUp: true);

            await command.Command.SetPermsToCategoryChannelsAsync(category, role, viewChannel);
        }
    }
}
