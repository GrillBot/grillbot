using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Data.Models.API.Users;
using Entities = GrillBot.Database.Entity;

namespace GrillBot.Data.Models.API.Searching
{
    public class SearchingListItem
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Who searching
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Server where was created search.
        /// </summary>
        public Guild Guild { get; set; }

        /// <summary>
        /// Channel where was created search.
        /// </summary>
        public Channel Channel { get; set; }

        /// <summary>
        /// Message content
        /// </summary>
        public string Message { get; set; }

        public SearchingListItem() { }

        public SearchingListItem(Entities.SearchItem entity)
        {
            Id = entity.Id;
            User = new User(entity.User);
            Guild = new Guild(entity.Guild);
            Channel = new Channel(entity.Channel);
            Message = entity.MessageContent;
        }
    }
}
