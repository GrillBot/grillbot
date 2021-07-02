using Discord;
using Discord.WebSocket;
using GrillBot.Data.Models;
using GrillBot.Data.Models.Unverify;
using GrillBot.Database.Entity;
using GrillBot.Database.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.App.Services.Unverify
{
    public class UnverifyProfileGenerator
    {
        private GrillBotContextFactory DbFactory { get; }

        public UnverifyProfileGenerator(GrillBotContextFactory dbFactory)
        {
            DbFactory = dbFactory;
        }

        public async Task<UnverifyUserProfile> CreateAsync(SocketGuildUser user, SocketGuild guild, DateTime end, string data, bool selfunverify, List<string> keep, IRole mutedRole)
        {
            var profile = new UnverifyUserProfile(user, DateTime.Now, end, selfunverify) { Reason = ParseReason(data) };

            // TODO: Selfunverify
            ProcessRoles(profile, user, guild, selfunverify, keep, mutedRole);
            ProcessChannels(profile, guild, user, keep);

            return profile;
        }

        public UnverifyUserProfile Reconstruct(UnverifyLog log, IGuildUser toUser, SocketGuild guild)
        {
            var logData = JsonConvert.DeserializeObject<UnverifyLogSet>(log.Data);

            return new UnverifyUserProfile(toUser, logData.Start, logData.End, logData.IsSelfUnverify)
            {
                ChannelsToKeep = logData.ChannelsToKeep,
                ChannelsToRemove = logData.ChannelsToRemove,
                Reason = logData.Reason,
                RolesToKeep = logData.RolesToKeep.Select(o => guild.GetRole(o) as IRole).Where(o => o != null).ToList(),
                RolesToRemove = logData.RolesToRemove.Select(o => guild.GetRole(o) as IRole).Where(o => o != null).ToList()
            };
        }

        private static string ParseReason(string data)
        {
            var ex = new ValidationException("Nelze bezdůvodně odebrat přístup. Přečti si nápovědu a pak to zkus znovu.");

            var parts = data.Split("<@", StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) throw ex;

            var reason = parts[0].Trim();
            if (string.IsNullOrEmpty(reason)) throw ex;
            return reason;
        }

        private void ProcessRoles(UnverifyUserProfile profile, SocketGuildUser user, SocketGuild guild, bool selfunverify, List<string> keep, IRole mutedRole)
        {
            profile.RolesToRemove.AddRange(user.Roles.Where(o => !o.IsEveryone));

            if (selfunverify)
            {
                var botRolePosition = guild.CurrentUser.Roles.Max(o => o.Position);
                var rolesToKeep = profile.RolesToRemove.FindAll(o => o.Position >= botRolePosition);

                profile.RolesToKeep.AddRange(rolesToKeep);
                profile.RolesToRemove.RemoveAll(o => rolesToKeep.Any(x => x.Id == o.Id));
            }

            var unavailable = profile.RolesToRemove.FindAll(o => o.IsManaged || (mutedRole != null && o.Id == mutedRole.Id));
            profile.RolesToKeep.AddRange(unavailable);
            profile.RolesToRemove.RemoveAll(o => unavailable.Any(x => x.Id == o.Id));

            // TODO: Selfunverify roles keeping.
        }

        private void ProcessChannels(UnverifyUserProfile profile, SocketGuild guild, SocketGuildUser user, List<string> keep)
        {
            var channels = guild.Channels.Where(o => o is SocketTextChannel || o is SocketVoiceChannel);

            var channelsToRemove = channels
                .Select(o => new ChannelOverride(o, o.GetPermissionOverwrite(user) ?? OverwritePermissions.InheritAll))
                .Where(o => o.AllowValue > 0 || o.DenyValue > 0);

            profile.ChannelsToRemove.AddRange(channelsToRemove);

            // TODO: Selfunverify channels keeping.
        }
    }
}
