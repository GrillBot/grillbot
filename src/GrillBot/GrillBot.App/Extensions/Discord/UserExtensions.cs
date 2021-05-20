using Discord;
using Discord.WebSocket;

namespace GrillBot.App.Extensions.Discord
{
    static public class UserExtensions
    {
        static public string GetDisplayName(this IUser user, bool withDiscriminator = true)
        {
            if (user is SocketGuildUser sgu && !string.IsNullOrEmpty(sgu.Nickname))
                return sgu.Nickname;

            return withDiscriminator ? $"{user.Username}#{user.Discriminator}" : user.Username;
        }

        static public string GetAvatarUri(this IUser user, ImageFormat format = ImageFormat.Auto, ushort size = 128)
        {
            return user.GetAvatarUrl(format, size) ?? user.GetDefaultAvatarUrl();
        }

        static public bool IsUser(this IUser user) => !(user.IsBot || user.IsWebhook);
    }
}
