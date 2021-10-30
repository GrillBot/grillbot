using Discord;
using Discord.WebSocket;
using GrillBot.App.Extensions;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Infrastructure.TypeReaders;
using GrillBot.App.Services.MessageCache;
using GrillBot.Data.Helpers;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Params;
using GrillBot.Database.Enums;
using GrillBot.Database.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.Annotations;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace GrillBot.App.Controllers
{
    [ApiController]
    [Route("api/channel")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [OpenApiTag("Channels", Description = "Channel management")]
    public class ChannelController : Controller
    {
        private DiscordSocketClient DiscordClient { get; }
        private GrillBotContext DbContext { get; }
        private MessageCache MessageCache { get; }

        public ChannelController(DiscordSocketClient discordClient, GrillBotContext dbContext, MessageCache messageCache)
        {
            DiscordClient = discordClient;
            DbContext = dbContext;
            MessageCache = messageCache;
        }

        /// <summary>
        /// Sends message to channel.
        /// </summary>
        /// <param name="guildId">Guild ID</param>
        /// <param name="channelId">Channel ID</param>
        /// <param name="parameters"></param>
        /// <response code="200">Success</response>
        /// <response code="404">Guild or channel not exists</response>
        [HttpPost("{guildId}/{channelId}")]
        [OpenApiOperation(nameof(ChannelController) + "_" + nameof(SendMessageToChannelAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> SendMessageToChannelAsync(ulong guildId, ulong channelId, [FromBody] SendMessageToChannelParams parameters)
        {
            var guild = DiscordClient.GetGuild(guildId);

            if (guild == null)
                return NotFound(new MessageResponse("Nepodařilo se najít server."));

            var channel = guild.GetTextChannel(channelId);

            if (channel == null)
                return NotFound(new MessageResponse($"Nepodařilo se najít kanál na serveru {guild.Name}"));

            MessageReference reference = null;
            if (!string.IsNullOrEmpty(parameters.Reference))
            {
                if (ulong.TryParse(parameters.Reference, out ulong messageId))
                    reference = new MessageReference(messageId, channelId, guildId);

                if (reference == null && Uri.IsWellFormedUriString(parameters.Reference, UriKind.Absolute))
                {
                    var messageUriMatch = MessageTypeReader.DiscordUriRegex.Match(parameters.Reference);

                    if (messageUriMatch.Success)
                        reference = new MessageReference(Convert.ToUInt64(messageUriMatch.Groups[3].Value), channelId, guildId);
                }
            }

            await channel.SendMessageAsync(parameters.Content, messageReference: reference);
            return Ok();
        }

        /// <summary>
        /// Gets paginated list of channels.
        /// </summary>
        [HttpGet]
        [OpenApiOperation(nameof(ChannelController) + "_" + nameof(GetChannelsListAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<PaginatedResponse<GuildChannel>>> GetChannelsListAsync([FromQuery] GetChannelListParams parameters)
        {
            var query = DbContext.Channels.AsNoTracking()
                .Include(o => o.Guild)
                .AsQueryable();

            query = parameters.CreateQuery(query);
            var result = await PaginatedResponse<GuildChannel>.CreateAsync(query, parameters, entity => new(entity));
            return Ok(result);
        }

        /// <summary>
        /// Removes all messages in message cache.
        /// </summary>
        [HttpDelete("{guildId}/{channelId}/cache")]
        [OpenApiOperation(nameof(ChannelController) + "_" + nameof(ClearChannelCacheAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult> ClearChannelCacheAsync(ulong guildId, ulong channelId)
        {
            var clearedCount = MessageCache.ClearChannel(channelId);
            var guild = DiscordClient.GetGuild(guildId);
            var channel = guild?.GetTextChannel(channelId);

            if (guild != null)
                await DbContext.InitGuildAsync(guild, CancellationToken.None);
            if (channel != null)
                await DbContext.InitGuildChannelAsync(guild, channel, DiscordHelper.GetChannelType(channel).Value, CancellationToken.None);

            var userId = User.GetUserId();
            var user = await DiscordClient.FindUserAsync(userId);

            await DbContext.InitGuildAsync(guild, CancellationToken.None);
            await DbContext.InitGuildChannelAsync(guild, channel, ChannelType.Text, CancellationToken.None);
            await DbContext.InitUserAsync(user, CancellationToken.None);

            var logItem = Database.Entity.AuditLogItem.Create(AuditLogItemType.Info, guild, channel, user,
                $"Uživatel vyčistil memory cache kanálu. Počet smazaných zpráv z cache je {clearedCount}");
            await DbContext.AddAsync(logItem);
            await DbContext.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Gets detail of channel.
        /// </summary>
        /// <param name="id">ID</param>
        /// <response code="200">Success</response>
        /// <response code="404">Channel not found.</response>
        [HttpGet("{id}")]
        [OpenApiOperation(nameof(ChannelController) + "_" + nameof(GetChannelDetailAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<ChannelDetail>> GetChannelDetailAsync(ulong id)
        {
            var channel = await DbContext.Channels.AsNoTracking()
                .Include(o => o.Guild)
                .FirstOrDefaultAsync(o => o.ChannelId == id.ToString());

            if (channel == null)
                return NotFound(new MessageResponse("Požadovaný kanál nebyl nalezen."));

            var userChannelsQuery = DbContext.UserChannels.AsNoTracking()
                .Where(o => o.Id == id.ToString());

            var channelDetailQuery = userChannelsQuery.GroupBy(o => o.Id)
                .Select(o => new
                {
                    MessagesCount = o.Sum(x => x.Count),
                    FirstMessageAt = o.Min(x => x.FirstMessageAt),
                    LastMessageAt = o.Max(x => x.LastMessageAt)
                });

            var channelDetailData = await channelDetailQuery.FirstOrDefaultAsync();

            var mostActiveUser = await userChannelsQuery.OrderByDescending(o => o.Count)
                .Select(o => o.User.User).FirstOrDefaultAsync();

            var lastMessageFrom = await userChannelsQuery.OrderByDescending(o => o.LastMessageAt)
                .Select(o => o.User.User).FirstOrDefaultAsync();

            var channelDetail = new ChannelDetail(channel)
            {
                FirstMessageAt = channelDetailData?.FirstMessageAt,
                LastMessageAt = channelDetailData?.LastMessageAt,
                LastMessageFrom = lastMessageFrom == null ? null : new(lastMessageFrom),
                MessagesCount = channelDetailData?.MessagesCount ?? 0,
                MostActiveUser = mostActiveUser == null ? null : new(mostActiveUser),
            };

            return Ok(channelDetail);
        }

        /// <summary>
        /// Gets paginated list of user statistics in channels.
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="400">Validation failed</response>
        [HttpGet("{id}/userStats")]
        [OpenApiOperation(nameof(ChannelController) + "_" + nameof(GetChannelUsersAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<PaginatedResponse<ChannelUserStatItem>>> GetChannelUsersAsync(ulong id, [FromQuery] PaginatedParams pagination)
        {
            var query = DbContext.UserChannels.AsNoTracking()
                .Include(o => o.User).ThenInclude(o => o.User)
                .OrderByDescending(o => o.Count)
                .Where(o => o.Id == id.ToString());

            var result = await PaginatedResponse<ChannelUserStatItem>.CreateAsync(query, pagination, entity => new()
            {
                Count = entity.Count,
                FirstMessageAt = entity.FirstMessageAt,
                LastMessageAt = entity.LastMessageAt,
                Nickname = entity.User.Nickname,
                UserId = entity.UserId,
                Username = entity.User.User.Username
            });

            for (int i = 0; i < result.Data.Count; i++) result.Data[i].Position = pagination.Skip + i + 1;
            return Ok(result);
        }
    }
}
