using GrillBot.App.Extensions.Discord;
using GrillBot.App.Infrastructure.TypeReaders.Implementations;
using GrillBot.App.Services.MessageCache;
using GrillBot.Data.Helpers;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Params;
using GrillBot.Database.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.Annotations;
using GrillBot.App.Extensions;

namespace GrillBot.App.Controllers
{
    [ApiController]
    [Route("api/channel")]
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
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
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
                    var messageUriMatch = MessageConverter.DiscordUriRegex.Match(parameters.Reference);

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
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [OpenApiOperation(nameof(ChannelController) + "_" + nameof(GetChannelsListAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<PaginatedResponse<GuildChannel>>> GetChannelsListAsync([FromQuery] GetChannelListParams parameters)
        {
            var query = DbContext.Channels.AsNoTracking().AsSplitQuery()
                .Include(o => o.Guild)
                .Include(o => o.Users)
                .AsQueryable();

            query = parameters.CreateQuery(query);
            var result = await PaginatedResponse<GuildChannel>.CreateAsync(query, parameters, entity =>
            {
                var cachedMessagesCount = MessageCache.GetMessagesFromChannel(Convert.ToUInt64(entity.ChannelId)).Count();
                return new(entity, cachedMessagesCount);
            });
            return Ok(result);
        }

        /// <summary>
        /// Removes all messages in message cache.
        /// </summary>
        [HttpDelete("{guildId}/{channelId}/cache")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
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
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [OpenApiOperation(nameof(ChannelController) + "_" + nameof(GetChannelDetailAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<ChannelDetail>> GetChannelDetailAsync(ulong id)
        {
            var channel = await DbContext.Channels.AsNoTracking()
                .Include(o => o.Guild)
                .Include(o => o.Users)
                .Include(o => o.ParentChannel)
                .FirstOrDefaultAsync(o => o.ChannelId == id.ToString());

            if (channel == null)
                return NotFound(new MessageResponse("Požadovaný kanál nebyl nalezen."));

            var userChannelsQuery = DbContext.UserChannels.AsNoTracking()
                .Where(o => o.Id == id.ToString());

            var mostActiveUser = await userChannelsQuery.OrderByDescending(o => o.Count)
                .Select(o => o.User.User).FirstOrDefaultAsync();

            var lastMessageFrom = await userChannelsQuery.OrderByDescending(o => o.LastMessageAt)
                .Select(o => o.User.User).FirstOrDefaultAsync();

            var channelDetail = new ChannelDetail(channel)
            {
                LastMessageFrom = lastMessageFrom == null ? null : new(lastMessageFrom),
                MostActiveUser = mostActiveUser == null ? null : new(mostActiveUser),
                CachedMessagesCount = MessageCache.GetMessagesFromChannel(Convert.ToUInt64(channel.ChannelId)).Count()
            };

            return Ok(channelDetail);
        }

        /// <summary>
        /// Gets paginated list of user statistics in channels.
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="400">Validation failed</response>
        [HttpGet("{id}/userStats")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
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

        /// <summary>
        /// Gets channelboard for channels where user have access.
        /// </summary>
        /// <response code="200">Success</response>
        [HttpGet("board")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [OpenApiOperation(nameof(ChannelController) + "_" + nameof(GetChannelboardAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ChannelboardItem>>> GetChannelboardAsync()
        {
            var userId = User.GetUserId();
            var mutualGuilds = DiscordClient.FindMutualGuilds(userId).ToList();
            var channelboard = new List<ChannelboardItem>();

            foreach (var guild in mutualGuilds)
            {
                var guildUser = guild.GetUser(userId);
                var availableChannels = guild.GetAvailableTextChannelsFor(guildUser)
                    .Select(o => o.Id.ToString()).ToList();

                if (availableChannels.Count == 0) continue;

                var channelsDataQuery = DbContext.UserChannels.AsNoTracking()
                    .Where(o => o.Count > 0 && o.GuildId == guild.Id.ToString())
                    .AsQueryable();

                var groupedChannelsQuery = channelsDataQuery.GroupBy(o => o.Id).Select(o => new
                {
                    ChannelId = o.Key,
                    Count = o.Sum(x => x.Count),
                    LastMessageAt = o.Max(x => x.LastMessageAt),
                    FirstMessageAt = o.Min(x => x.FirstMessageAt)
                });

                var groupedChannels = await groupedChannelsQuery.ToListAsync();
                if (groupedChannels.Count == 0) continue;

                var channelsQuery = DbContext.Channels.AsNoTracking()
                    .Include(o => o.Guild)
                    .Where(o => o.GuildId == guild.Id.ToString())
                    .AsQueryable();

                foreach (var channelData in groupedChannels.Where(o => availableChannels.Contains(o.ChannelId)))
                {
                    var channel = await channelsQuery.FirstOrDefaultAsync(o => o.ChannelId == channelData.ChannelId);
                    if (channel == null) continue;

                    channelboard.Add(new ChannelboardItem(channel, channelData.Count, channelData.LastMessageAt, channelData.FirstMessageAt));
                }
            }

            channelboard = channelboard
                .OrderByDescending(o => o.Count)
                .ThenByDescending(o => o.LastMessageAt)
                .ToList();

            return Ok(channelboard);
        }
    }
}
