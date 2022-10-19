using System.Diagnostics.CodeAnalysis;
using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;
using ModuleBase = GrillBot.App.Infrastructure.Commands.ModuleBase;

namespace GrillBot.App.Modules.TextBased;

[ExcludeFromCodeCoverage]
public class ServerModule : ModuleBase
{
    [Command("clean")]
    [TextCommandDeprecated(AlternativeCommand = "/channel clean")]
    public Task CleanAsync(int take, ITextChannel channel = null) => Task.CompletedTask;

    [Group("pin")]
    [TextCommandDeprecated(AlternativeCommand = "/pin purge")]
    public class PinManagementSubmodule : ModuleBase
    {
        [Command("purge")]
        public Task PurgePinsAsync(ITextChannel channel = null, params ulong[] messageIds) => Task.CompletedTask;

        [Command("purge")]
        public Task PurgePinsAsync(int count, ITextChannel channel = null) => Task.CompletedTask;
    }

    [Group("guild")]
    public class GuildManagementSubmodule : ModuleBase
    {
        [Command("send")]
        [TextCommandDeprecated(AlternativeCommand = "/channel send")]
        public Task SendAnonymousToChannelAsync(IMessageChannel channel, string content = null) => Task.CompletedTask;

        [Command("info")]
        [TextCommandDeprecated(AlternativeCommand = "/guild info")]
        public Task InfoAsync() => Task.CompletedTask;

        [Group("perms")]
        public class GuildPermissionsSubModule : ModuleBase
        {
            [Command("clear")]
            [TextCommandDeprecated(AlternativeCommand = "/permissions remove all")]
            public Task ClearPermissionsInChannelAsync(IGuildChannel channel, params IUser[] excludedUsers) => Task.CompletedTask;

            [Group("useless")]
            public class GuildUselessPermissionsSubModule : ModuleBase
            {
                [Command("check")]
                [TextCommandDeprecated(AlternativeCommand = "/permissions useless check")]
                public Task CheckUselessPermissionsAsync() => Task.CompletedTask;

                [Command("clear")]
                [TextCommandDeprecated(AlternativeCommand = "/permissions useless clear")]
                public Task RemoveUselessPermissionsAsync(Guid? sessionId = null) => Task.CompletedTask;
            }
        }

        [Group("react")]
        public class GuildReactSubModule : ModuleBase
        {
            [Command("clear")]
            [TextCommandDeprecated(AlternativeCommand = "/message clear react")]
            public Task RemoveReactionAsync(IMessage message, IEmote emote) => Task.CompletedTask;
        }

        [Group("role")]
        public class GuildRolesSubModule : ModuleBase
        {
            [Group("info")]
            public class GuildRoleInfoSubModule : ModuleBase
            {
                [Command("")]
                [Alias("position")]
                [TextCommandDeprecated(AlternativeCommand = "/role list")]
                public Task GetRoleInfoListByPositionAsync() => Task.CompletedTask;

                [Command("members")]
                [TextCommandDeprecated(AlternativeCommand = "/role list")]
                public Task GetRoleInfoListByMemberCountAsync() => Task.CompletedTask;

                [Command("")]
                [TextCommandDeprecated(AlternativeCommand = "/role get")]
                public Task GetRoleInfoAsync(SocketRole role) => Task.CompletedTask;
            }
        }
    }
}
