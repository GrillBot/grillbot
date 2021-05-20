using Discord;
using System;
using System.Linq;

namespace GrillBot.App.Helpers
{
    static public class DiscordHelper
    {
        static public GatewayIntents GetAllIntents()
        {
            return Enum.GetValues<GatewayIntents>().Aggregate((result, next) => result | next);
        }
    }
}
