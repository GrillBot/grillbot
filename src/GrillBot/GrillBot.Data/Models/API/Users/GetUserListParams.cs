using GrillBot.Data.Models.API.Params;
using System.Linq;

namespace GrillBot.Data.Models.API.Users
{
    public class GetUserListParams : PaginatedParams
    {
        /// <summary>
        /// Username.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Selected guild.
        /// </summary>
        public string GuildId { get; set; }

        /// <summary>
        /// Flag that describe user have API access.
        /// </summary>
        public bool HaveApiAccess { get; set; }

        /// <summary>
        /// Selected flags from UserFlags enum.
        /// </summary>
        public long? Flags { get; set; }

        /// <summary>
        /// Select users that have stored birthday.
        /// </summary>
        public bool HaveBirthday { get; set; }

        /// <summary>
        /// Sort direction.
        /// </summary>
        public bool SortDesc { get; set; }

        public IQueryable<Database.Entity.User> CreateQuery(IQueryable<Database.Entity.User> query)
        {
            if (!string.IsNullOrEmpty(Username))
                query = query.Where(o => o.Username.Contains(Username));

            if (!string.IsNullOrEmpty(GuildId))
                query = query.Where(o => o.Guilds.Any(x => x.GuildId == GuildId));

            if (HaveApiAccess)
                query = query.Where(o => o.ApiToken != null);

            if (Flags != null)
                query = query.Where(o => (o.Flags & Flags) == Flags);

            if (HaveBirthday)
                query = query.Where(o => o.Birthday != null);

            return SortDesc switch
            {
                true => query.OrderByDescending(o => o.Username),
                _ => query.OrderBy(o => o.Username)
            };
        }
    }
}
