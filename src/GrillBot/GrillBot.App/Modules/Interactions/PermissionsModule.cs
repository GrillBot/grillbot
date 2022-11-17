using System.Diagnostics.CodeAnalysis;
using Discord.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.Common.Extensions.Discord;

namespace GrillBot.App.Modules.Interactions;

[Group("permissions", "Manage channel permissions.")]
[RequireUserPerms]
[ExcludeFromCodeCoverage]
public class PermissionsModule : InteractionsModuleBase
{
    public PermissionsModule() : base(null)
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
        public async Task ClearPermissionsFromChannelAsync(IGuildChannel channel, IEnumerable<IUser> excludedUsers = null)
        {
            var users = excludedUsers?.Select(o => o as IGuildUser ?? Context.Guild.GetUser(o.Id)).Where(o => o != null).ToList();

            using var command = GetCommand<Actions.Commands.PermissionsCleaner>();
            command.Command.OnProgress = async progressBar => await SetResponseAsync(progressBar, suppressFollowUp: true);
            await command.Command.ClearAllPermissionsAsync(channel, users);
        }
    }

    [Group("useless", "Useless permissions processing")]
    [RequireBotPermission(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
    public class UselessPermissionsSubModule : InteractionsModuleBase
    {
        public UselessPermissionsSubModule(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        [SlashCommand("check", "Check for useless permissions")]
        public async Task CheckUselessPermissionsAsync()
        {
            using var command = GetCommand<Actions.Commands.PermissionsReader>();

            var uselessPermissions = await command.Command.ReadUselessPermissionsAsync();
            var summary = command.Command.CreateSummary(uselessPermissions);

            if (uselessPermissions.Count == 0)
            {
                await SetResponseAsync(summary);
                return;
            }

            var values = uselessPermissions.ConvertAll(o => $"> #{o.Channel.Name} - {o.User.GetFullName()} - {o.Type}");
            var filename = $"UselessPermissions_{Context.Guild.Id}_{DateTime.Now:yyyyMMdd}.txt";
            var jsonBytes = Encoding.UTF8.GetBytes(string.Join("\n", values));
            var attachment = new FileAttachment(new MemoryStream(jsonBytes), filename);

            try
            {
                await SetResponseAsync(summary, attachments: new[] { attachment });
            }
            finally
            {
                attachment.Dispose();
            }
        }

        [SlashCommand("clear", "Remove useless permissions")]
        public async Task RemoveUselessPermissionsAsync()
        {
            using var command = GetCommand<Actions.Commands.PermissionsCleaner>();
            command.Command.PermissionsReader.Init(Context);

            command.Command.OnProgress = async progressBar => await SetResponseAsync(progressBar, suppressFollowUp: true);
            await command.Command.RemoveUselessPermissionsAsync();
        }
    }
}
