using Discord.WebSocket;
using GrillBot.App.Extensions.Discord;
using GrillBot.Data.Models;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Public;
using GrillBot.Database.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NSwag.Annotations;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GrillBot.App.Controllers.Public
{
    [ApiController]
    [Route("api/public/leaderboard")]
    [OpenApiTag("Public", Description = "API endpoints to leaderboards.")]
    public class LeaderboardController : ControllerBase
    {
        private IMemoryCache MemoryCache { get; }
        private GrillBotContextFactory DbFactory { get; }
        private DiscordSocketClient DiscordClient { get; }

        public LeaderboardController(IMemoryCache memoryCache, GrillBotContextFactory dbFactory, DiscordSocketClient discordClient)
        {
            MemoryCache = memoryCache;
            DbFactory = dbFactory;
            DiscordClient = discordClient;
        }

        /// <summary>
        /// Gets complete list of channelboard.
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        /// <response code="200">OK</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="404">Session, guild or user not found.</response>
        [HttpGet("channelboard/{sessionId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<Channelboard>> GetChannelboardAsync([Required] string sessionId)
        {
            if (!MemoryCache.TryGetValue<ChannelboardWebMetadata>(sessionId, out var metadata))
                return NotFound(new MessageResponse("Požadované sezení nebylo nalezeno."));

            var guild = DiscordClient.GetGuild(metadata.GuildId);
            if (guild == null)
                return NotFound(new MessageResponse("Server nacházející se v sezení nebyl nalezen."));

            await guild.DownloadUsersAsync();
            var user = guild.GetUser(metadata.UserId);
            if (user == null)
                return NotFound(new MessageResponse("Uživatel přiřazený k sezení nebyl nalezen."));

            var availableChannels = guild.GetAvailableChannelsFor(user).Select(o => o.Id.ToString()).ToList();

            using var dbContext = DbFactory.Create();

            var query = dbContext.Channels.AsQueryable()
                .Where(o => o.GuildId == metadata.GuildId.ToString() && availableChannels.Contains(o.Id) && o.Count > 0)
                .GroupBy(o => new { o.GuildId, o.Id }).Select(o => new
                {
                    ChannelId = o.Key.Id,
                    Count = o.Sum(x => x.Count),
                    LastMessageAt = o.Max(x => x.LastMessageAt)
                })
                .OrderByDescending(o => o.Count)
                .ThenByDescending(o => o.LastMessageAt);

            var channelboard = new Channelboard(sessionId, guild, user);
            channelboard.Channels.AddRange((await query.ToListAsync()).Select(o =>
            {
                var channel = guild.GetTextChannel(Convert.ToUInt64(o.ChannelId));
                return new ChannelboardItem(channel, o.Count, o.LastMessageAt);
            }));

            return Ok(channelboard);
        }
    }
}
