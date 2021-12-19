using Discord;
using Discord.WebSocket;
using GrillBot.Data.Models.API.Statistics;
using GrillBot.Data.Models.API.System;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using GrillBot.Database.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NSwag.Annotations;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GrillBot.App.Controllers
{
    [ApiController]
    [Route("api/system")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [OpenApiTag("System", Description = "Internal system management, ...")]
    public class SystemController : Controller
    {
        private IWebHostEnvironment Environment { get; }
        private DiscordSocketClient DiscordClient { get; }
        private GrillBotContext DbContext { get; }

        public SystemController(IWebHostEnvironment environment, DiscordSocketClient discordClient,
            GrillBotContext dbContext)
        {
            Environment = environment;
            DiscordClient = discordClient;
            DbContext = dbContext;
        }

        /// <summary>
        /// Gets diagnostics data about application.
        /// </summary>
        /// <response code="200">Success</response>
        [HttpGet("diag")]
        [OpenApiOperation(nameof(SystemController) + "_" + nameof(GetDiagnostics))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult<DiagnosticsInfo> GetDiagnostics()
        {
            var data = new DiagnosticsInfo(Environment.EnvironmentName, DiscordClient);
            return Ok(data);
        }

        /// <summary>
        /// Gets infromation about database tables.
        /// </summary>
        [HttpGet("db")]
        [OpenApiOperation(nameof(SystemController) + "_" + nameof(GetDbStatusAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<Dictionary<string, int>>> GetDbStatusAsync()
        {
            var data = new Dictionary<string, int>()
            {
                { nameof(DbContext.Users), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.Users) },
                { nameof(DbContext.Guilds), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.Guilds) },
                { nameof(DbContext.GuildUsers), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.GuildUsers) },
                { nameof(DbContext.Channels), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.Channels) },
                { nameof(DbContext.UserChannels), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.UserChannels) },
                { nameof(DbContext.Invites), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.Invites) },
                { nameof(DbContext.SearchItems), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.SearchItems) },
                { nameof(DbContext.Unverifies), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.Unverifies) },
                { nameof(DbContext.UnverifyLogs), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.UnverifyLogs) },
                { nameof(DbContext.AuditLogs), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.AuditLogs) },
                { nameof(DbContext.AuditLogFiles), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.AuditLogFiles) },
                { nameof(DbContext.Emotes), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.Emotes) },
                { nameof(DbContext.Reminders), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.Reminders) },
                { nameof(DbContext.SelfunverifyKeepables), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.SelfunverifyKeepables) },
                { nameof(DbContext.ExplicitPermissions), await EntityFrameworkQueryableExtensions.CountAsync(DbContext.ExplicitPermissions) }
            };

            return Ok(data);
        }

        /// <summary>
        /// Gets statistics about commands.
        /// </summary>
        [HttpGet("commands")]
        [OpenApiOperation(nameof(SystemController) + "_" + nameof(GetCommandStatusAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<CommandStatisticItem>>> GetCommandStatusAsync(string searchQuery = null)
        {
            var query = DbContext.AuditLogs.AsNoTracking()
                .Where(o => o.Type == AuditLogItemType.Command)
                .Select(o => new { o.CreatedAt, o.Data });

            var dbData = await query.ToListAsync();
            var deserializedData = dbData.ConvertAll(o => new
            {
                o.CreatedAt,
                Data = JsonConvert.DeserializeObject<CommandExecution>(o.Data)
            });

            var dataQuery = deserializedData.Where(o => o.Data.Command != null);
            if (!string.IsNullOrEmpty(searchQuery))
                dataQuery = dataQuery.Where(o => o.Data.Command.Contains(searchQuery));

            var groupedData = dataQuery
                .GroupBy(o => o.Data.Command)
                .Select(o => new CommandStatisticItem()
                {
                    Command = o.Key,
                    FailedCount = o.Count(x => !x.Data.IsSuccess),
                    LastCall = o.Max(x => x.CreatedAt),
                    SuccessCount = o.Count(x => x.Data.IsSuccess)
                })
                .OrderBy(o => o.Command)
                .ToList();

            return Ok(groupedData);
        }

        /// <summary>
        /// Changes bot account status and set bot's status activity.
        /// </summary>
        /// <response code="200">Success</response>
        [HttpPut("status/{status}")]
        [OpenApiOperation(nameof(SystemController) + "_" + nameof(ChangeBotStatusAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult> ChangeBotStatusAsync(UserStatus status)
        {
            if (status == UserStatus.Offline) status = UserStatus.Invisible;
            if (status == UserStatus.AFK) status = UserStatus.Idle;

            await DiscordClient.SetStatusAsync(status);
            return Ok();
        }
    }
}
