using Discord.WebSocket;
using GrillBot.Data.Models.API.System;
using GrillBot.Database.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.Annotations;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace GrillBot.App.Controllers
{
    [ApiController]
    [Route("api/system")]
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
                { nameof(DbContext.Users), await DbContext.Users.CountAsync() },
                { nameof(DbContext.Guilds), await DbContext.Guilds.CountAsync() },
                { nameof(DbContext.GuildUsers), await DbContext.GuildUsers.CountAsync() },
                { nameof(DbContext.Channels), await DbContext.Channels.CountAsync() },
                { nameof(DbContext.UserChannels), await DbContext.UserChannels.CountAsync() },
                { nameof(DbContext.Invites), await DbContext.Invites.CountAsync() },
                { nameof(DbContext.SearchItems), await DbContext.SearchItems.CountAsync() },
                { nameof(DbContext.Unverifies), await DbContext.Unverifies.CountAsync() },
                { nameof(DbContext.UnverifyLogs), await DbContext.UnverifyLogs.CountAsync() },
                { nameof(DbContext.AuditLogs), await DbContext.AuditLogs.CountAsync() },
                { nameof(DbContext.AuditLogFiles), await DbContext.AuditLogFiles.CountAsync() },
                { nameof(DbContext.Emotes), await DbContext.Emotes.CountAsync() },
                { nameof(DbContext.Reminders), await DbContext.Reminders.CountAsync() },
                { nameof(DbContext.SelfunverifyKeepables), await DbContext.SearchItems.CountAsync() },
                { nameof(DbContext.ExplicitPermissions), await DbContext.ExplicitPermissions.CountAsync() }
            };

            return Ok(data);
        }
    }
}
