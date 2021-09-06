using GrillBot.Data.Models.API.Params;
using System;
using System.Linq;

namespace GrillBot.Data.Models.API.Reminder
{
    public class GetReminderListParams : PaginatedParams
    {
        public string FromUserId { get; set; }
        public string ToUserId { get; set; }
        public string OriginalMessageId { get; set; }
        public string MessageContains { get; set; }

        /// <summary>
        /// Available: Id, FromUser, ToUser, At, Postpone
        /// </summary>
        public string SortBy { get; set; } = "Id";

        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public bool SortDesc { get; set; }

        public IQueryable<Database.Entity.RemindMessage> CreateQuery(IQueryable<Database.Entity.RemindMessage> query)
        {
            if (!string.IsNullOrEmpty(FromUserId))
                query = query.Where(o => o.FromUserId == FromUserId);

            if (!string.IsNullOrEmpty(ToUserId))
                query = query.Where(o => o.ToUserId == ToUserId);

            if (!string.IsNullOrEmpty(OriginalMessageId))
                query = query.Where(o => o.OriginalMessageId == OriginalMessageId);

            if (!string.IsNullOrEmpty(MessageContains))
                query = query.Where(o => o.Message.Contains(MessageContains));

            if (CreatedFrom != null)
                query = query.Where(o => o.At >= CreatedFrom.Value);

            if (CreatedTo != null)
                query = query.Where(o => o.At <= CreatedTo.Value);

            return SortBy.ToLower() switch
            {
                "fromuser" => SortDesc switch
                {
                    true => query.OrderByDescending(o => o.FromUser.Username).ThenByDescending(o => o.Id),
                    _ => query.OrderBy(o => o.FromUser.Username).ThenBy(o => o.Id)
                },
                "touser" => SortDesc switch
                {
                    true => query.OrderByDescending(o => o.ToUser.Username).ThenByDescending(o => o.Id),
                    _ => query.OrderBy(o => o.ToUser.Username).ThenBy(o => o.Id)
                },
                "at" => SortDesc switch
                {
                    true => query.OrderByDescending(o => o.At).ThenByDescending(o => o.Id),
                    _ => query.OrderBy(o => o.At).ThenBy(o => o.At)
                },
                _ => SortDesc switch
                {
                    true => query.OrderByDescending(o => o.Id),
                    _ => query.OrderBy(o => o.Id)
                }
            };
        }
    }
}
