﻿using Discord;
using System.Net.Http;
using System.Threading.Tasks;

namespace GrillBot.App.Extensions.Discord
{
    static public class UserExtensions
    {
        static public string GetDisplayName(this IUser user, bool withDiscriminator = true)
        {
            if (user is IGuildUser sgu && !string.IsNullOrEmpty(sgu.Nickname))
                return sgu.Nickname;

            return withDiscriminator ? $"{user.Username}#{user.Discriminator}" : user.Username;
        }

        static public string GetAvatarUri(this IUser user, ImageFormat format = ImageFormat.Auto, ushort size = 128)
        {
            return user.GetAvatarUrl(format, size) ?? user.GetDefaultAvatarUrl();
        }

        static public bool IsUser(this IUser user) => !(user.IsBot || user.IsWebhook);

        static public string GetFullName(this IUser user)
        {
            if (user is IGuildUser sgu && !string.IsNullOrEmpty(sgu.Nickname))
                return $"{sgu.Nickname} ({sgu.Username}#{sgu.Discriminator})";

            return $"{user.Username}#{user.Discriminator}";
        }

        static public async Task<byte[]> DownloadAvatarAsync(this IUser user, ImageFormat format = ImageFormat.Auto, ushort size = 128)
        {
            var url = user.GetAvatarUri(format, size);

            using var client = new HttpClient();
            return await client.GetByteArrayAsync(url);
        }

        static public bool HaveAnimatedAvatar(this IUser user) => user.AvatarId.StartsWith("a_");
    }
}
