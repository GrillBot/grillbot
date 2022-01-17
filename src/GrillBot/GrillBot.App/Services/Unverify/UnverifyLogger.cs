using Discord;
using Discord.WebSocket;
using GrillBot.Data.Infrastructure;
using GrillBot.Data.Models;
using GrillBot.Data.Models.Unverify;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Database.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GrillBot.Data.Services.Unverify
{
    public class UnverifyLogger : ServiceBase
    {
        public UnverifyLogger(DiscordSocketClient client, GrillBotContextFactory dbFactory) : base(client, dbFactory)
        {
        }

        public Task<UnverifyLog> LogUnverifyAsync(UnverifyUserProfile profile, IGuild guild, IGuildUser from, CancellationToken cancellationToken)
        {
            var data = UnverifyLogSet.FromProfile(profile);
            return SaveAsync(UnverifyOperation.Unverify, data, from, guild, profile.Destination, cancellationToken);
        }

        public Task<UnverifyLog> LogSelfunverifyAsync(UnverifyUserProfile profile, IGuild guild, CancellationToken cancellationToken)
        {
            var data = UnverifyLogSet.FromProfile(profile);
            return SaveAsync(UnverifyOperation.Selfunverify, data, profile.Destination, guild, profile.Destination, cancellationToken);
        }

        public async Task LogAutoremoveAsync(List<IRole> returnedRoles, List<ChannelOverride> returnedChannels, IGuildUser toUser, IGuild guild,
            CancellationToken cancellationToken)
        {
            var data = new UnverifyLogRemove()
            {
                ReturnedOverwrites = returnedChannels,
                ReturnedRoles = returnedRoles.ConvertAll(o => o.Id)
            };

            var currentUser = await guild.GetUserAsync(DiscordClient.CurrentUser.Id);
            await SaveAsync(UnverifyOperation.Autoremove, data, currentUser, guild, toUser, cancellationToken);
        }

        public Task LogRemoveAsync(List<IRole> returnedRoles, List<ChannelOverride> returnedChannels, IGuild guild, IGuildUser from, IGuildUser to,
            CancellationToken cancellationToken)
        {
            var data = new UnverifyLogRemove()
            {
                ReturnedOverwrites = returnedChannels,
                ReturnedRoles = returnedRoles.ConvertAll(o => o.Id)
            };

            return SaveAsync(UnverifyOperation.Remove, data, from, guild, to, cancellationToken);
        }

        public Task LogUpdateAsync(DateTime start, DateTime end, IGuild guild, IGuildUser from, IGuildUser to, CancellationToken cancellationToken)
        {
            var data = new UnverifyLogUpdate()
            {
                End = end,
                Start = start
            };

            return SaveAsync(UnverifyOperation.Update, data, from, guild, to, cancellationToken);
        }

        public Task LogRecoverAsync(List<IRole> returnedRoles, List<ChannelOverride> returnedChannels, IGuild guild, IGuildUser from, IGuildUser to,
            CancellationToken cancellationToken)
        {
            var data = new UnverifyLogRemove()
            {
                ReturnedOverwrites = returnedChannels,
                ReturnedRoles = returnedRoles.ConvertAll(o => o.Id)
            };

            return SaveAsync(UnverifyOperation.Recover, data, from, guild, to, cancellationToken);
        }

        private async Task<UnverifyLog> SaveAsync(UnverifyOperation operation, object data, IGuildUser from, IGuild guild, IGuildUser toUser,
            CancellationToken cancellationToken)
        {
            var entity = new UnverifyLog()
            {
                CreatedAt = DateTime.Now,
                Data = JsonConvert.SerializeObject(data),
                FromUserId = from.Id.ToString(),
                GuildId = guild.Id.ToString(),
                Operation = operation,
                ToUserId = toUser.Id.ToString()
            };

            using var context = DbFactory.Create();

            await context.InitGuildAsync(guild, cancellationToken);
            await context.InitUserAsync(from, cancellationToken);
            await context.InitGuildUserAsync(guild, from, cancellationToken);

            if (from != toUser)
            {
                await context.InitUserAsync(toUser, cancellationToken);
                await context.InitGuildUserAsync(guild, toUser, cancellationToken);
            }

            await context.AddAsync(entity, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return entity;
        }
    }
}
