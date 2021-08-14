using System;

namespace GrillBot.Data.Models.API.AuditLog
{
    public class AuditLogStatItem
    {
        public string StatName { get; set; }
        public int Count { get; set; }
        public DateTime? FirstItem { get; set; }
        public DateTime? LastItem { get; set; }

        public AuditLogStatItem() { }

        public AuditLogStatItem(string name, int? count, DateTime? firstItem, DateTime? lastItem)
        {
            StatName = name.Trim();
            Count = count ?? 0;
            FirstItem = firstItem;
            LastItem = lastItem;
        }
    }
}
