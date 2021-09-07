using Discord;
using GrillBot.Data.Models.API.Params;
using System.Collections.Generic;
using System.Linq;

namespace GrillBot.Data.Models.API.Channels
{
    public class GetChannelListParams : PaginatedParams
    {
        public string GuildId { get; set; }
        public string NameContains { get; set; }
        public List<ChannelType> ChannelTypes { get; set; }

        /// <summary>
        /// Available options: Name, Type. Default is Name.
        /// </summary>
        public string SortBy { get; set; } = "Name";

        public bool SortDesc { get; set; }

        public IQueryable<Database.Entity.GuildChannel> CreateQuery(IQueryable<Database.Entity.GuildChannel> query)
        {
            if (!string.IsNullOrEmpty(GuildId))
                query = query.Where(o => o.GuildId == GuildId);

            if (!string.IsNullOrEmpty(NameContains))
                query = query.Where(o => o.Name.Contains(NameContains));

            if (ChannelTypes?.Count > 0)
                query = query.Where(o => ChannelTypes.Contains(o.ChannelType));

            return SortBy.ToLower() switch
            {
                "type" => SortDesc switch
                {
                    true => query.OrderByDescending(o => o.ChannelType).ThenByDescending(o => o.Name),
                    _ => query.OrderBy(o => o.ChannelType).ThenBy(o => o.Name)
                },
                _ => SortDesc switch
                {
                    true => query.OrderByDescending(o => o.Name),
                    _ => query.OrderBy(o => o.Name)
                }
            };
        }
    }
}
