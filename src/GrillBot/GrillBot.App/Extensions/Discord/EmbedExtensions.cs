using Discord;
using GrillBot.Data.Extensions.Discord;

namespace GrillBot.App.Extensions.Discord
{
    static public class EmbedExtensions
    {
        static public EmbedBuilder WithFooter(this EmbedBuilder builder, IUser user)
        {
            return builder.WithFooter(user.GetDisplayName(), user.GetAvatarUri());
        }
    }
}
