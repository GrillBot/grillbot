using GrillBot.App.Infrastructure;
using GrillBot.Data.Models;
using GrillBot.Data.Models.Unverify;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.Unverify
{
    public class UnverifyLogger : ServiceBase
    {
        public UnverifyLogger(DiscordSocketClient client, GrillBotDatabaseBuilder dbFactory) : base(client, dbFactory)
        {
        }

        public Task<UnverifyLog> LogUnverifyAsync(UnverifyUserProfile profile, IGuild guild, IGuildUser from)
        {
            var data = UnverifyLogSet.FromProfile(profile);
            return SaveAsync(UnverifyOperation.Unverify, data, from, guild, profile.Destination);
        }

        public Task<UnverifyLog> LogSelfunverifyAsync(UnverifyUserProfile profile, IGuild guild)
        {
            var data = UnverifyLogSet.FromProfile(profile);
            return SaveAsync(UnverifyOperation.Selfunverify, data, profile.Destination, guild, profile.Destination);
        }

        public async Task LogAutoremoveAsync(List<IRole> returnedRoles, List<ChannelOverride> returnedChannels, IGuildUser toUser, IGuild guild)
        {
            var data = new UnverifyLogRemove()
            {
                ReturnedOverwrites = returnedChannels,
                ReturnedRoles = returnedRoles.ConvertAll(o => o.Id)
            };

            var currentUser = await guild.GetUserAsync(DiscordClient.CurrentUser.Id);
            await SaveAsync(UnverifyOperation.Autoremove, data, currentUser, guild, toUser);
        }

        public Task LogRemoveAsync(List<IRole> returnedRoles, List<ChannelOverride> returnedChannels, IGuild guild, IGuildUser from, IGuildUser to)
        {
            var data = new UnverifyLogRemove()
            {
                ReturnedOverwrites = returnedChannels,
                ReturnedRoles = returnedRoles.ConvertAll(o => o.Id)
            };

            return SaveAsync(UnverifyOperation.Remove, data, from, guild, to);
        }

        public Task LogUpdateAsync(DateTime start, DateTime end, IGuild guild, IGuildUser from, IGuildUser to)
        {
            var data = new UnverifyLogUpdate()
            {
                End = end,
                Start = start
            };

            return SaveAsync(UnverifyOperation.Update, data, from, guild, to);
        }

        public Task LogRecoverAsync(List<IRole> returnedRoles, List<ChannelOverride> returnedChannels, IGuild guild, IGuildUser from, IGuildUser to)
        {
            var data = new UnverifyLogRemove()
            {
                ReturnedOverwrites = returnedChannels,
                ReturnedRoles = returnedRoles.ConvertAll(o => o.Id)
            };

            return SaveAsync(UnverifyOperation.Recover, data, from, guild, to);
        }

        private async Task<UnverifyLog> SaveAsync(UnverifyOperation operation, object data, IGuildUser from, IGuild guild, IGuildUser toUser)
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

            await context.InitGuildAsync(guild);
            await context.InitUserAsync(from);
            await context.InitGuildUserAsync(guild, from);

            if (from != toUser)
            {
                await context.InitUserAsync(toUser);
                await context.InitGuildUserAsync(guild, toUser);
            }

            await context.AddAsync(entity);
            await context.SaveChangesAsync();

            return entity;
        }
    }
}
