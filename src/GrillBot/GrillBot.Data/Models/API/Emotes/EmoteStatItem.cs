using Discord;
using System;

namespace GrillBot.Data.Models.API.Emotes
{
    public class EmoteStatItem
    {
        public string Name { get; set; }
        public long UseCount { get; set; }
        public string ImageUrl { get; set; }
        public DateTime FirstOccurence { get; set; }
        public DateTime LastOccurence { get; set; }

        public EmoteStatItem() { }

        public EmoteStatItem(Database.Entity.EmoteStatisticItem item)
        {
            var emote = Emote.Parse(item.EmoteId);

            Name = emote.Name;
            UseCount = item.UseCount;
            ImageUrl = emote.Url;
            FirstOccurence = item.FirstOccurence;
            LastOccurence = item.LastOccurence;
        }
    }
}
