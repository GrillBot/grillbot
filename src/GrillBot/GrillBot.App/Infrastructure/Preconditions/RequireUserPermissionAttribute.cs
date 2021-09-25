using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GrillBot.Database.Enums;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.App.Infrastructure.Preconditions
{
    /// <summary>
    /// Permission system to control command running.
    /// You can define where bot can execute command, next you can define which guild or channel permissions will be required.
    /// Next you can allow server boosters to use this command and disallow explicit permissions.
    /// Bot administrators have full permissions on all commands. On this users will not work disallow explicit.
    /// </summary>
    /// <remarks>
    /// Order of checks:
    /// 1) Checks for contexts, if specified. If success, it will do next checks.
    /// 2) If user is bot admin, checks ends with success.
    /// 3) If command not have disabled explicit permissions for users, checks ends with fail.
    /// 4) If command is trying execute in guild, it will check guild or channel permissions. If command is trying execute in DMs, it will check channel permissions. If success, checks ends with success.
    /// 5) If command is trying execute in guild and command is enabled for booster it will check if user is server booster (have premium role). If success, checks ends with success.
    /// </remarks>
    public class RequireUserPermissionAttribute : PreconditionAttribute
    {
        /// <summary>
        /// Run command in specific channel types (DM/Guild). Null allows run command everywhere.
        /// </summary>
        public ContextType? Contexts { get; set; }

        /// <summary>
        /// Run command with specific guild permissions.
        /// </summary>
        public GuildPermission[] GuildPermissions { get; }

        /// <summary>
        /// Run command with specific channel permissions.
        /// </summary>
        public ChannelPermission[] ChannelPermissions { get; }

        /// <summary>
        /// Allow command for server booster.
        /// </summary>
        public bool AllowBooster { get; }

        /// <summary>
        /// Disallow explicit allow permissions to command.
        /// </summary>
        public bool DisallowExplicit { get; }

        private RequireUserPermissionAttribute(bool booster, ContextType? contexts, bool disallowExplicit)
        {
            AllowBooster = booster;
            Contexts = contexts;
            DisallowExplicit = disallowExplicit;
        }

        /// <summary>
        /// Allow command use for users with specific guild permissions (AND logic combinations betweeen guild permissions) or boosters.
        /// If you want, you can disallow explicit permissions.
        /// </summary>
        public RequireUserPermissionAttribute(GuildPermission[] guildPermissions, bool booster, bool disallowExplicit = false) : this(booster, ContextType.Guild, disallowExplicit)
        {
            GuildPermissions = guildPermissions;
        }

        public RequireUserPermissionAttribute(ChannelPermission[] channelPermissions, bool booster, bool disallowExplicit = false) : this(booster, booster ? ContextType.Guild : null, disallowExplicit)
        {
            ChannelPermissions = channelPermissions;
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var contextCheck = await CheckContextsAsync(context, command, services);
            if (!contextCheck.IsSuccess) return contextCheck;

            var botAdminCheck = await CheckBotAdministratorPermsAsync(context, services);
            if (botAdminCheck.IsSuccess) return PreconditionResult.FromSuccess();

            var explicitBanCheck = await CheckExplicitPermissionBans(context, command, services);
            if (!explicitBanCheck.IsSuccess) return explicitBanCheck;

            var guildPermsCheck = await CheckGuildPermissionsAsync(context, command, services);
            if (guildPermsCheck.IsSuccess) return PreconditionResult.FromSuccess();

            var channelPermsCheck = await CheckChannelPermsAsync(context, command, services);
            if (channelPermsCheck.IsSuccess) return PreconditionResult.FromSuccess();

            var boosterCheck = await CheckBoosterPermsAsync(context);
            if (boosterCheck.IsSuccess) return PreconditionResult.FromSuccess();

            var explicitCheck = await CheckExplicitPermissionAsync(context, command, services);
            if (explicitCheck.IsSuccess) return PreconditionResult.FromSuccess();

            var checkedPerms = new[]
            {
                guildPermsCheck.ErrorReason,
                channelPermsCheck.ErrorReason,
                boosterCheck.ErrorReason,
                explicitCheck.ErrorReason,
                botAdminCheck.ErrorReason
            }.Where(o => o != null && o != "-").ToList();
            var perms = string.Join("\n", checkedPerms.ConvertAll(o => $"> {o}"));

            if (checkedPerms.Count == 1)
                return PreconditionResult.FromError(checkedPerms[0]);

            return PreconditionResult.FromError($"Byly provedeny následující kontroly a ani jedna ti nedovoluje provést příkaz:\n{perms}");
        }

        private async Task<PreconditionResult> CheckContextsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (Contexts == null)
                return PreconditionResult.FromSuccess();

            var attribute = new RequireContextAttribute(Contexts.Value);
            var result = await attribute.CheckPermissionsAsync(context, command, services);

            if (result.IsSuccess)
                return result;

            if ((Contexts.Value & ContextType.Guild) != 0)
                return PreconditionResult.FromError("Tento příkaz lze použít pouze na serveru.");
            else
                return PreconditionResult.FromError("Tento příkaz lze použít pouze v soukromé konverzaci.");
        }

        private async Task<PreconditionResult> CheckGuildPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (GuildPermissions == null) return PreconditionResult.FromError("-");

            var invalidPerms = new List<GuildPermission>();
            foreach (var perm in GuildPermissions)
            {
                var attribute = new Discord.Commands.RequireUserPermissionAttribute(perm);
                var result = await attribute.CheckPermissionsAsync(context, command, services);

                if (!result.IsSuccess)
                    invalidPerms.Add(perm);
            }

            if (invalidPerms.Count > 0)
            {
                var formatedPerms = invalidPerms.Select(o => o switch
                {
                    GuildPermission.AddReactions => "přidávat reakce",
                    GuildPermission.Administrator => "administrátora",
                    GuildPermission.AttachFiles => "nahrávat soubory",
                    GuildPermission.BanMembers => "banovat uživatele",
                    GuildPermission.ChangeNickname => "měnit si přezdívku",
                    GuildPermission.Connect => "připojovat se do kanálů",
                    GuildPermission.CreateInstantInvite => "vytvářet pozvánky",
                    GuildPermission.DeafenMembers or GuildPermission.MuteMembers => "umlčet uživatele",
                    GuildPermission.EmbedLinks => "posílat odkazy",
                    GuildPermission.KickMembers => "vyhazovat uživatele",
                    GuildPermission.ManageChannels => "spravovat kanály",
                    GuildPermission.ManageEmojis => "spravovat emotikony",
                    GuildPermission.ManageGuild => "spravovat server",
                    GuildPermission.ManageMessages => "spravovat zprávy",
                    GuildPermission.ManageNicknames => "měnit ostatním přezdívky",
                    GuildPermission.ManageRoles => "spravovat role",
                    GuildPermission.ManageWebhooks => "spravovat webhooky",
                    GuildPermission.MentionEveryone => "tagovat všechny",
                    GuildPermission.MoveMembers => "přesouvat uživatele v hlasových kanálech",
                    GuildPermission.PrioritySpeaker => "mít prioritní hlas v hovoru",
                    GuildPermission.ReadMessageHistory => "číst historii zpráv",
                    GuildPermission.SendMessages => "posílat zprávy",
                    GuildPermission.SendTTSMessages => "posílat TTS zprávy",
                    GuildPermission.Speak => "mluvit v hlasovém kanálu",
                    GuildPermission.Stream => "streamovat",
                    GuildPermission.UseExternalEmojis => "používat externí emotikony",
                    GuildPermission.ViewAuditLog => "vidět logy",
                    GuildPermission.ViewChannel => "číst zprávy v kanálu",
                    GuildPermission.ViewGuildInsights => "vidět statistiky serveru",
                    _ => "nějaké jiné právo"
                }).Distinct().ToList();

                var perms = string.Join(", ", formatedPerms.Take(formatedPerms.Count - 1));
                return PreconditionResult.FromError($"Na tento příkaz nemáš oprávnění, protože nemáš oprávnění {perms}{(formatedPerms.Count > 1 ? " a " : null)}{formatedPerms[^1]}.");
            }

            return PreconditionResult.FromSuccess();
        }

        private async Task<PreconditionResult> CheckChannelPermsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (ChannelPermissions == null) return PreconditionResult.FromError("-");

            var invalidPerms = new List<ChannelPermission>();
            foreach (var perm in ChannelPermissions)
            {
                var attribute = new Discord.Commands.RequireUserPermissionAttribute(perm);
                var result = await attribute.CheckPermissionsAsync(context, command, services);

                if (!result.IsSuccess)
                    invalidPerms.Add(perm);
            }

            if (invalidPerms.Count > 0)
            {
                var formatedPerms = invalidPerms.Select(o => o switch
                {
                    ChannelPermission.AddReactions => "přidávat reakce",
                    ChannelPermission.AttachFiles => "nahrávat soubory",
                    ChannelPermission.Connect => "volat",
                    ChannelPermission.CreateInstantInvite => "vytvářet pozvánky",
                    ChannelPermission.DeafenMembers => "umlčet uživatele",
                    ChannelPermission.EmbedLinks => "posílat odkazy",
                    ChannelPermission.ManageChannels => "spravovat kanál",
                    ChannelPermission.ManageMessages => "spravovat zprávy",
                    ChannelPermission.ManageRoles => "spravovat přístupy",
                    ChannelPermission.ManageWebhooks => "spravovat webhooky",
                    ChannelPermission.MentionEveryone => "tagovat všechny",
                    ChannelPermission.MoveMembers => "přesouvat uživatele v hlasových kanálech",
                    ChannelPermission.MuteMembers => "umlčet uživatele",
                    ChannelPermission.PrioritySpeaker => "mít prioritní hlas v hovoru",
                    ChannelPermission.ReadMessageHistory => "číst historii",
                    ChannelPermission.SendMessages => "posílat zprávy",
                    ChannelPermission.SendTTSMessages => "posílat TTS zprávy",
                    ChannelPermission.Speak => "mluvit v hlasovém kanálu",
                    ChannelPermission.Stream => "streamovat",
                    ChannelPermission.UseExternalEmojis => "používat externí emotikony",
                    ChannelPermission.ViewChannel => "číst zprávy v kanálu",
                    _ => "nějaké jiné právo"
                }).Distinct().ToList();

                var perms = string.Join(", ", formatedPerms.Skip(formatedPerms.Count - 1));
                return PreconditionResult.FromError($"Na tento příkaz nemáš oprávnění, protože nemáš oprávnění na {perms} a {formatedPerms[^1]}.");
            }

            return PreconditionResult.FromSuccess();
        }

        private Task<PreconditionResult> CheckBoosterPermsAsync(ICommandContext context)
        {
            if (!AllowBooster) return Task.FromResult(PreconditionResult.FromError("-"));

            if (context.User is not SocketGuildUser user)
                return Task.FromResult(PreconditionResult.FromError("Nevoláš tento příkaz na serveru."));

            if (!user.Roles.Any(o => o.Tags?.IsPremiumSubscriberRole == true))
                return Task.FromResult(PreconditionResult.FromError("Nejsi server booster."));

            return Task.FromResult(PreconditionResult.FromSuccess());
        }

        private async Task<PreconditionResult> CheckExplicitPermissionAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (DisallowExplicit) return PreconditionResult.FromError("-");

            var dbFactory = services.GetRequiredService<GrillBotContextFactory>();
            using var dbContext = dbFactory.Create();

            var permissions = await dbContext.ExplicitPermissions
                .AsNoTracking()
                .Where(o => o.Command == command.Aliases[0].Trim() && o.State == ExplicitPermissionState.Allowed)
                .ToListAsync();

            if (permissions.Count == 0)
                return PreconditionResult.FromError("-");

            // It user explicit permission not found and user is in guild.
            if (!permissions.Any(o => !o.IsRole && o.TargetId == context.User.Id.ToString()) && context.User is SocketGuildUser user)
            {
                foreach (var role in user.Roles)
                {
                    if (permissions.Any(o => o.IsRole && o.TargetId == role.Id.ToString()))
                        return PreconditionResult.FromSuccess();
                }
            }
            else
            {
                return PreconditionResult.FromSuccess();
            }

            return PreconditionResult.FromError("Pro tento příkaz nemáš žádné explicitní povolení.");
        }

        private async Task<PreconditionResult> CheckExplicitPermissionBans(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (DisallowExplicit) return PreconditionResult.FromSuccess();

            var dbFactory = services.GetRequiredService<GrillBotContextFactory>();
            using var dbContext = dbFactory.Create();

            var permissionExists = await dbContext.ExplicitPermissions
                .AsNoTracking()
                .AnyAsync(o => o.Command == command.Aliases[0].Trim() && o.State == ExplicitPermissionState.Banned && !o.IsRole && o.TargetId == context.User.Id.ToString());

            if (!permissionExists)
                return PreconditionResult.FromSuccess();
            else
                return PreconditionResult.FromError("Tento příkaz nemůžeš použít. Byl ti k němu zakázán přístup.");
        }

        private static async Task<PreconditionResult> CheckBotAdministratorPermsAsync(ICommandContext context, IServiceProvider services)
        {
            var dbFactory = services.GetRequiredService<GrillBotContextFactory>();
            using var dbContext = dbFactory.Create();

            var isBotAdmin = await dbContext.Users.AsNoTracking()
                .AnyAsync(o => o.Id == context.User.Id.ToString() && (o.Flags & (int)UserFlags.BotAdmin) != 0);

            if (!isBotAdmin)
                return PreconditionResult.FromError("Nejsi administrátor bota.");

            return PreconditionResult.FromSuccess();
        }
    }
}
