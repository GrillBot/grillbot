using GrillBot.Data.Models.API.Params;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GrillBot.Data.Models.API.Unverify
{
    /// <summary>
    /// Paginated params of unverify logs
    /// </summary>
    public class UnverifyLogParams : PaginatedParams
    {
        /// <summary>
        /// Selected operations.
        /// </summary>
        public UnverifyOperation? Operation { get; set; }

        /// <summary>
        /// Guild ID
        /// </summary>
        public string GuildId { get; set; }

        /// <summary>
        /// Who did operation. If user have lower permission, this property is ignored.
        /// </summary>
        public string FromUserId { get; set; }

        /// <summary>
        /// Who was target of operation. If user have lower permission, this property is ignored.
        /// </summary>
        public string ToUserId { get; set; }

        /// <summary>
        /// Start of range when operation did.
        /// </summary>
        public DateTime? CreatedFrom { get; set; }

        /// <summary>
        /// End of range when operation did.
        /// </summary>
        public DateTime? CreatedTo { get; set; }

        /// <summary>
        /// Sorting (Values are Operation, Guild, FromUser, ToUser, CreatedAt). Default is CreatedAt.
        /// </summary>
        public string SortBy { get; set; } = "CreatedAt";

        /// <summary>
        /// Descending sort. If false, ascending sort will be used.
        /// </summary>
        public bool SortDesc { get; set; }

        public IQueryable<UnverifyLog> CreateQuery(IQueryable<UnverifyLog> query)
        {
            if (Operation != null)
                query = query.Where(o => o.Operation == Operation);

            if (!string.IsNullOrEmpty(GuildId))
                query = query.Where(o => o.GuildId == GuildId);

            if (!string.IsNullOrEmpty(FromUserId))
                query = query.Where(o => o.FromUserId == FromUserId);

            if (!string.IsNullOrEmpty(ToUserId))
                query = query.Where(o => o.ToUserId == ToUserId);

            if (CreatedFrom != null)
                query = query.Where(o => o.CreatedAt >= CreatedFrom.Value);

            if (CreatedTo != null)
                query = query.Where(o => o.CreatedAt <= CreatedTo.Value);

            return SortBy.ToLower() switch
            {
                "operation" => SortDesc switch
                {
                    true => query.OrderByDescending(o => o.Operation).ThenByDescending(o => o.Id),
                    _ => query.OrderBy(o => o.Operation).ThenBy(o => o.Id)
                },
                "guild" => SortDesc switch
                {
                    true => query.OrderByDescending(o => o.Guild.Name).ThenByDescending(o => o.Id),
                    _ => query.OrderBy(o => o.Guild.Name).ThenBy(o => o.Id)
                },
                "fromuser" => SortDesc switch
                {
                    true => query.OrderByDescending(o => o.FromUser.User.Username).ThenByDescending(o => o.FromUser.User.Discriminator).ThenByDescending(o => o.Id),
                    _ => query.OrderBy(o => o.FromUser.User.Username).ThenBy(o => o.FromUser.User.Discriminator).ThenBy(o => o.Id)
                },
                "touser" => SortDesc switch
                {
                    true => query.OrderByDescending(o => o.ToUser.User.Username).ThenByDescending(o => o.ToUser.User.Discriminator).ThenByDescending(o => o.Id),
                    _ => query.OrderBy(o => o.ToUser.User.Username).ThenBy(o => o.ToUser.User.Discriminator).ThenBy(o => o.Id)
                },
                _ => SortDesc switch
                {
                    true => query.OrderByDescending(o => o.CreatedAt).ThenByDescending(o => o.Id),
                    _ => query.OrderBy(o => o.CreatedAt).ThenBy(o => o.Id)
                },
            };
        }
    }
}
