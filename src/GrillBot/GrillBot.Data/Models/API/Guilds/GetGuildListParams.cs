using GrillBot.Data.Models.API.Common;
using System.Linq;

namespace GrillBot.Data.Models.API.Guilds
{
    public class GetGuildListParams : PaginatedParams
    {
        /// <summary>
        /// Query over name of guild.
        /// </summary>
        public string NameQuery { get; set; }

        public IQueryable<Database.Entity.Guild> CreateQuery(IQueryable<Database.Entity.Guild> query)
        {
            if (!string.IsNullOrEmpty(NameQuery))
                query = query.Where(o => o.Name.Contains(NameQuery));

            return query;
        }
    }
}
