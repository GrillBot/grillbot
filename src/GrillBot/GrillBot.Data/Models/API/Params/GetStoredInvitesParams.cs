using InviteEntity = GrillBot.Database.Entity.Invite;
using System;
using System.Linq;

namespace GrillBot.Data.Models.API.Params
{
    public class GetStoredInvitesParams : PaginatedParams<InviteEntity>
    {
        public string CreatorId { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedTo { get; set; }

        public bool SortDescending { get; set; }

        public override IQueryable<InviteEntity> CreateQuery(IQueryable<InviteEntity> query)
        {
            if (!string.IsNullOrEmpty(CreatorId))
                query = query.Where(o => o.CreatorId == CreatorId);

            if (CreatedFrom != null)
                query = query.Where(o => o.CreatedAt >= CreatedFrom.Value);

            if (CreatedTo != null)
                query = query.Where(o => o.CreatedAt < CreatedTo.Value);

            query = SortDescending
                ? query.OrderByDescending(o => o.UsedUsers.Count).ThenByDescending(o => o.CreatedAt).ThenBy(o => o.Code)
                : query.OrderBy(o => o.UsedUsers.Count).ThenBy(o => o.CreatedAt).ThenBy(o => o.Code);

            return base.CreateQuery(query);
        }
    }
}
