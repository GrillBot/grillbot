using GrillBot.Data.Models.API.Params;
using GrillBot.Database.Entity;
using System.Linq;

namespace GrillBot.Data.Models.API.Searching
{
    public class GetSearchingListParams : PaginatedParams
    {
        /// <summary>
        /// Id of user who searching.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Server Id
        /// </summary>
        public string GuildId { get; set; }

        /// <summary>
        /// Channel Id
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// Substring contained in message.
        /// </summary>
        public string MessageQuery { get; set; }

        /// <summary>
        /// Sorting (Id, User, Guild, Channel)
        /// </summary>
        public string SortBy { get; set; } = "Id";

        /// <summary>
        /// Ascending or descending sort.
        /// </summary>
        public bool SortDesc { get; set; }

        public IQueryable<SearchItem> CreateQuery(IQueryable<SearchItem> query)
        {
            if (!string.IsNullOrEmpty(UserId))
                query = query.Where(o => o.UserId == UserId);

            if (!string.IsNullOrEmpty(GuildId))
                query = query.Where(o => o.GuildId == GuildId);

            if (!string.IsNullOrEmpty(ChannelId))
                query = query.Where(o => o.ChannelId == ChannelId);

            if (!string.IsNullOrEmpty(MessageQuery))
                query = query.Where(o => o.MessageContent.Contains(MessageQuery));

            return SortBy.ToLower() switch
            {
                "user" => SortDesc switch
                {
                    true => query.OrderByDescending(o => o.User.Username).ThenByDescending(o => o.User.Discriminator).ThenByDescending(o => o.Id),
                    _ => query.OrderBy(o => o.User.Username).ThenBy(o => o.User.Discriminator).ThenBy(o => o.Id)
                },
                "guild" => SortDesc switch
                {
                    true => query.OrderByDescending(o => o.Guild.Name).ThenByDescending(o => o.Id),
                    _ => query.OrderBy(o => o.Guild.Name).ThenBy(o => o.Id)
                },
                "channel" => SortDesc switch
                {
                    true => query.OrderByDescending(o => o.Channel.Name).ThenByDescending(o => o.Id),
                    _ => query.OrderBy(o => o.Channel.Name).ThenBy(o => o.Id)
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
