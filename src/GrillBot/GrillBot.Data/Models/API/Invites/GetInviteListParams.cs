using GrillBot.Data.Models.API.Params;
using System;
using System.Linq;

namespace GrillBot.Data.Models.API.Invites
{
    public class GetInviteListParams : PaginatedParams
    {
        public string GuildId { get; set; }
        public string CreatorId { get; set; }
        public string Code { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }

        /// <summary>
        /// Available values: Code, CreatedAt, Creator.
        /// </summary>
        public string SortBy { get; set; } = "Code";

        public bool SortDesc { get; set; }

        public IQueryable<Database.Entity.Invite> CreateQuery(IQueryable<Database.Entity.Invite> query)
        {
            if (!string.IsNullOrEmpty(GuildId))
                query = query.Where(o => o.GuildId == GuildId);

            if (!string.IsNullOrEmpty(CreatorId))
                query = query.Where(o => o.CreatorId == CreatorId);

            if (!string.IsNullOrEmpty(Code))
                query = query.Where(o => o.Code.Contains(Code));

            if (CreatedFrom != null)
                query = query.Where(o => o.CreatedAt >= CreatedFrom.Value);

            if (CreatedTo != null)
                query = query.Where(o => o.CreatedAt <= CreatedTo.Value);

            return SortBy.ToLower() switch
            {
                "createdat" => SortDesc switch
                {
                    true => query.OrderByDescending(o => o.CreatedAt),
                    _ => query.OrderBy(o => o.CreatedAt)
                },
                "creator" => SortDesc switch
                {
                    true => query.OrderByDescending(o => !string.IsNullOrEmpty(o.Creator.Nickname) ? o.Creator.Nickname : o.Creator.User.Username),
                    _ => query.OrderBy(o => !string.IsNullOrEmpty(o.Creator.Nickname) ? o.Creator.Nickname : o.Creator.User.Username),
                },
                _ => SortDesc switch
                {
                    true => query.OrderByDescending(o => o.Code),
                    _ => query.OrderBy(o => o.Code)
                }
            };
        }
    }
}
