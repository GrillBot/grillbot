using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GrillBot.App.Infrastructure;
using GrillBot.Data.Models.API.Statistics;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GrillBot.App.Services
{
    public class AuditLogService : ServiceBase
    {
        private GrillBotContextFactory DbFactory { get; }
        private JsonSerializerSettings JsonSerializerSettings { get; }

        public AuditLogService(DiscordSocketClient client, GrillBotContextFactory dbFactory) : base(client)
        {
            DbFactory = dbFactory;

            JsonSerializerSettings = new JsonSerializerSettings()
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        public async Task LogExecutedCommandAsync(CommandInfo command, ICommandContext context, IResult result)
        {
            var data = new CommandExecution(command, context.Message, result);

            using var dbContext = DbFactory.Create();

            await dbContext.InitGuildAsync(context.Guild);
            await dbContext.InitGuildUserAsync(context.Guild, context.User as IGuildUser);
            await dbContext.InitGuildChannelAsync(context.Guild, context.Channel);

            var logItem = new AuditLogItem()
            {
                ChannelId = context.Channel.Id.ToString(),
                CreatedAt = DateTime.Now,
                Data = JsonConvert.SerializeObject(data, JsonSerializerSettings),
                GuildId = context.Guild.Id.ToString(),
                ProcessedUserId = context.User.Id.ToString(),
                Type = AuditLogItemType.Command
            };

            await dbContext.AddAsync(logItem);
            await dbContext.SaveChangesAsync();
        }

        public async Task<List<AuditLogStatItem>> GetStatisticsAsync<TKey>(Expression<Func<AuditLogItem, TKey>> keySelector,
            Func<List<Tuple<TKey, int, DateTime, DateTime>>, IEnumerable<AuditLogStatItem>> converter)
        {
            using var dbContext = DbFactory.Create();

            var query = dbContext.AuditLogs.AsQueryable()
                .GroupBy(keySelector)
                .OrderBy(o => o.Key)
                .Select(o => new Tuple<TKey, int, DateTime, DateTime>(o.Key, o.Count(), o.Min(x => x.CreatedAt), o.Max(x => x.CreatedAt)));

            var data = await query.ToListAsync();
            return converter(data).ToList();
        }
    }
}
