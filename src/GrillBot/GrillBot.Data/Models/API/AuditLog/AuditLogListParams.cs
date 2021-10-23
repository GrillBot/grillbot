using GrillBot.Data.Models.API.Params;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GrillBot.Data.Models.API.AuditLog
{
    public class AuditLogListParams : PaginatedParams
    {
        /// <summary>
        /// Guild ID.
        /// </summary>
        public string GuildId { get; set; }

        /// <summary>
        /// Who processed operation.
        /// </summary>
        public string ProcessedUserId { get; set; }

        /// <summary>
        /// Types of operations.
        /// </summary>
        public List<AuditLogItemType> Types { get; set; }

        /// <summary>
        /// Start of range when operation did.
        /// </summary>
        public DateTime? CreatedFrom { get; set; }

        /// <summary>
        /// End of range when operation did.
        /// </summary>
        public DateTime? CreatedTo { get; set; }

        /// <summary>
        /// Ignore operations processed by bots.
        /// </summary>
        public bool IgnoreBots { get; set; }

        /// <summary>
        /// Id of channel where was processed operation.
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// Sorting (Values are Guild, Processed, Type, Channel, CreatedAt). Default is CreatedAt
        /// </summary>
        public string SortBy { get; set; } = "CreatedAt";

        /// <summary>
        /// Descending sort. If false, ascending sort will be used.
        /// </summary>
        public bool SortDesc { get; set; } = true;

        public IQueryable<AuditLogItem> CreateQuery(IQueryable<AuditLogItem> query)
        {
            if (!string.IsNullOrEmpty(GuildId))
                query = query.Where(o => o.GuildId == GuildId);

            if (!string.IsNullOrEmpty(ProcessedUserId))
                query = query.Where(o => o.ProcessedUserId == ProcessedUserId);

            if (Types?.Count > 0)
                query = query.Where(o => Types.Contains(o.Type));

            if (CreatedFrom != null)
                query = query.Where(o => o.CreatedAt >= CreatedFrom);

            if (CreatedTo != null)
                query = query.Where(o => o.CreatedAt <= CreatedTo);

            if (IgnoreBots)
                query = query.Where(o => o.ProcessedUserId == null || (o.ProcessedGuildUser.User.Flags & (int)UserFlags.NotUser) == 0);

            if (!string.IsNullOrEmpty(ChannelId))
                query = query.Where(o => o.ChannelId == ChannelId);

            return SortBy.ToLower() switch
            {
                "guild" => SortDesc switch
                {
                    true => query.OrderByDescending(o => o.Guild.Name).ThenByDescending(o => o.Id),
                    _ => query.OrderBy(o => o.Guild.Name).ThenBy(o => o.Id)
                },
                "processed" => SortDesc switch
                {
                    true => query.OrderByDescending(o => o.ProcessedGuildUser.Nickname).ThenByDescending(o => o.ProcessedGuildUser.User.Username).ThenByDescending(o => o.Id),
                    _ => query.OrderBy(o => o.ProcessedGuildUser.Nickname).ThenBy(o => o.ProcessedGuildUser.User.Username).ThenBy(o => o.Id)
                },
                "type" => SortDesc switch
                {
                    true => query.OrderByDescending(o => o.Type).ThenByDescending(o => o.Id),
                    _ => query.OrderBy(o => o.Type).ThenBy(o => o.Id)
                },
                "channel" => SortDesc switch
                {
                    true => query.OrderByDescending(o => o.GuildChannel.Name).ThenByDescending(o => o.Id),
                    _ => query.OrderBy(o => o.GuildChannel.Name).ThenBy(o => o.Id)
                },
                _ => SortDesc switch
                {
                    true => query.OrderByDescending(o => o.CreatedAt).ThenByDescending(o => o.Id),
                    _ => query.OrderBy(o => o.CreatedAt).ThenBy(o => o.Id)
                }
            };
        }
    }
}
