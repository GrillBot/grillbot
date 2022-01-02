using Discord;
using Discord.WebSocket;
using GrillBot.Data.Models;
using GrillBot.Data.Models.Unverify;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
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
            var profile = new UnverifyUserProfile(user, DateTime.Now, end, selfunverify) { Reason = !selfunverify ? ParseReason(data) : null };

            using var context = DbFactory.Create();
            var keepables = (await context.SelfunverifyKeepables.ToListAsync()).GroupBy(o => o.GroupName).ToDictionary(o => o.Key, o => o.Select(o => o.Name).ToList());

            ProcessRoles(profile, user, guild, selfunverify, keep, mutedRole, keepables);
            ProcessChannels(profile, guild, user, keep, keepables);

            return profile;
        }

        public UnverifyUserProfile Reconstruct(GrillBot.Database.Entity.Unverify unverify, IGuildUser toUser, SocketGuild guild)
        {
            var logData = JsonConvert.DeserializeObject<UnverifyLogSet>(unverify.UnverifyLog.Data);

            return new UnverifyUserProfile(toUser, unverify.StartAt, unverify.EndAt, logData.IsSelfUnverify)
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

        private static void ProcessRoles(UnverifyUserProfile profile, SocketGuildUser user, SocketGuild guild, bool selfunverify, List<string> keep, IRole mutedRole, Dictionary<string, List<string>> keepables)
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

            foreach (var toKeep in keep)
            {
                CheckDefinition(keepables, toKeep);
                var role = profile.RolesToRemove.Find(o => string.Equals(o.Name, toKeep, StringComparison.InvariantCultureIgnoreCase));

                if (role != null)
                {
                    profile.RolesToKeep.Add(role);
                    profile.RolesToRemove.Remove(role);
                    continue;
                }

                foreach (var groupKey in keepables.Where(o => o.Value?.Contains(toKeep) == true).Select(o => o.Key))
                {
                    role = profile.RolesToRemove.Find(o => string.Equals(o.Name, groupKey == "_" ? toKeep : groupKey, StringComparison.InvariantCultureIgnoreCase));

                    if (role != null)
                    {
                        profile.RolesToKeep.Add(role);
                        profile.RolesToRemove.Remove(role);
                    }
                }
            }
        }

        private static void ProcessChannels(UnverifyUserProfile profile, SocketGuild guild, SocketGuildUser user, List<string> keep, Dictionary<string, List<string>> keepabless)
        {
            var channels = guild.Channels
                .Where(o => (o is SocketTextChannel || o is SocketVoiceChannel) && o is not SocketThreadChannel); // Select channels but ignore channels

            var channelsToRemove = channels
                .Select(o => new ChannelOverride(o, o.GetPermissionOverwrite(user) ?? OverwritePermissions.InheritAll))
                .Where(o => o.AllowValue > 0 || o.DenyValue > 0);

            profile.ChannelsToRemove.AddRange(channelsToRemove);
            foreach (var toKeep in keep)
            {
                CheckDefinition(keepabless, toKeep);
                var overwrite = profile.ChannelsToRemove.Find(o => string.Equals(guild.GetChannel(o.ChannelId)?.Name, toKeep));

                if (overwrite != null)
                {
                    profile.ChannelsToKeep.Add(overwrite);
                    profile.ChannelsToRemove.RemoveAll(o => o.ChannelId == overwrite.ChannelId);
                }
            }
        }

        private static bool ExistsInKeepDefinition(Dictionary<string, List<string>> definitions, string item)
        {
            return definitions.ContainsKey(item) || definitions.Values.Any(o => o?.Contains(item) == true);
        }

        private static void CheckDefinition(Dictionary<string, List<string>> definitions, string item)
        {
            if (!ExistsInKeepDefinition(definitions, item))
                throw new ValidationException($"{item.ToLower()} není ponechatelné.");
        }
    }
}
