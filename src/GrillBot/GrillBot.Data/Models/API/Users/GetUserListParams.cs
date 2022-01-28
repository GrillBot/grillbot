using GrillBot.Data.Models.API.Params;
using Microsoft.EntityFrameworkCore;
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

        /// <summary>
        /// Used invite code
        /// </summary>
        public string UsedInviteCode { get; set; }

        public IQueryable<Database.Entity.User> CreateQuery(IQueryable<Database.Entity.User> query)
        {
            if (!string.IsNullOrEmpty(Username))
                query = query.Where(o => o.Username.Contains(Username));

            if (!string.IsNullOrEmpty(GuildId))
                query = query.Where(o => o.Guilds.Any(x => x.GuildId == GuildId));

            if (Flags != null)
                query = query.Where(o => (o.Flags & Flags) == Flags);

            if (HaveBirthday)
                query = query.Where(o => o.Birthday != null);

            if (!string.IsNullOrEmpty(UsedInviteCode))
                query = query.Where(o => o.Guilds.Any(x => EF.Functions.ILike(x.UsedInviteCode, $"{UsedInviteCode.ToLower()}%")));

            return SortDesc switch
            {
                true => query.OrderByDescending(o => o.Username).ThenByDescending(o => o.Discriminator),
                _ => query.OrderBy(o => o.Username).ThenBy(o => o.Discriminator)
            };
        }
    }
}
