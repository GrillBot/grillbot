using Discord;
using Discord.WebSocket;
using GrillBot.App.Extensions;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Services;
using GrillBot.Data.Models.API.Statistics;
using GrillBot.Database.Enums;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GrillBot.App.Controllers
{
    [ApiController]
    [Route("api/auditlog")]
    [OpenApiTag("Audit log", Description = "Logging")]
    public class AuditLogController : Controller
    {
        private AuditLogService AuditLogService { get; }
        private DiscordSocketClient DiscordClient { get; }

        public AuditLogController(AuditLogService auditLogService, DiscordSocketClient discordClient)
        {
            AuditLogService = auditLogService;
            DiscordClient = discordClient;
        }

        /// <summary>
        /// Gets nonpaginated list of audit log statistics by type.
        /// </summary>
        /// <response code="200">OK</response>
        [HttpGet("stats/type")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<AuditLogStatItem>>> GetStatisticsByTypeAsync()
        {
            var data = await AuditLogService.GetStatisticsAsync(o => o.Type, data =>
            {
                return Enum.GetValues<AuditLogItemType>().Select(o =>
                {
                    var item = data.Find(x => x.Item1 == o);
                    return new AuditLogStatItem(o.GetDisplayName(), item?.Item2, item?.Item3, item?.Item4);
                });
            });

            return Ok(data);
        }

        /// <summary>
        /// Gets nonpaginated list of audit log statistics by guild.
        /// </summary>
        /// <response code="200">OK</response>
        [HttpGet("stats/guild")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<AuditLogStatItem>>> GetStatisticsByGuildAsync()
        {
            var data = await AuditLogService.GetStatisticsAsync(o => o.GuildId, data =>
            {
                var result = DiscordClient.Guilds.Select(g =>
                {
                    var item = data.Find(o => o.Item1 == g.Id.ToString());
                    return new AuditLogStatItem(g.Name, item?.Item2, item?.Item3, item?.Item4);
                }).ToList();

                var unknown = data.Find(o => o.Item1 == null);
                if (unknown != null)
                    result.Add(new AuditLogStatItem("Neznámý server", unknown?.Item2, unknown?.Item3, unknown?.Item4));

                return result;
            });

            return Ok(data);
        }

        /// <summary>
        /// Gets nonpaginated list of audit log statistics by channel.
        /// </summary>
        /// <response code="200">OK</response>
        [HttpGet("stats/channel")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<AuditLogStatItem>>> GetStatisticsByChannelAsync()
        {
            var data = await AuditLogService.GetStatisticsAsync(o => o.ChannelId, data =>
            {
                return data.Select(o =>
                {
                    if (string.IsNullOrEmpty(o.Item1) || DiscordClient.GetChannel(Convert.ToUInt64(o.Item1)) is not IChannel channel)
                        return new AuditLogStatItem("Neznámý kanál", o.Item2, o.Item3, o.Item4);

                    return new AuditLogStatItem(channel.Name, o.Item2, o.Item3, o.Item4);
                }).Where(o => o != null).OrderBy(o => o.StatName == "Neznámý kanál" ? new string('z', byte.MaxValue) : o.StatName).ThenBy(o => o.Count);
            });

            return Ok(data);
        }

        /// <summary>
        /// Gets nonpaginated list of audit log statistics by user.
        /// </summary>
        /// <response code="200">OK</response>
        [HttpGet("stats/user")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<AuditLogStatItem>>> GetStatisticsByUserAsync()
        {
            foreach (var guild in DiscordClient.Guilds)
                await guild.DownloadUsersAsync();

            var data = await AuditLogService.GetStatisticsAsync(o => o.ProcessedUserId, data =>
            {
                return data.Select(o =>
                {
                    var user = DiscordClient.GetUser(Convert.ToUInt64(o.Item1));

                    if (string.IsNullOrEmpty(o.Item1) || user == null)
                        return new AuditLogStatItem($"Neznámý uživatel {(string.IsNullOrEmpty(o.Item1) ? "" : $"({o.Item1})")}", o.Item2, o.Item3, o.Item4);

                    return new AuditLogStatItem(user.GetFullName(), o.Item2, o.Item3, o.Item4);
                }).Where(o => o != null).OrderBy(o => o.StatName).ThenBy(o => o.Count);
            });

            return Ok(data);
        }
    }
}
