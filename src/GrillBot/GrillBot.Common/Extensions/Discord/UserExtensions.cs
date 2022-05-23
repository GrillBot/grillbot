using Discord;

namespace GrillBot.Common.Extensions.Discord;

public static class UserExtensions
{
    public static string GetUserAvatarUrl(this IUser user, ushort size = 128)
        => user.GetAvatarUrl(size: size) ?? user.GetDefaultAvatarUrl();

    public static async Task<byte[]> DownloadAvatarAsync(this IUser user, ushort size = 128)
    {
        var url = GetUserAvatarUrl(user, size);

        using var httpClient = new HttpClient();
        return await httpClient.GetByteArrayAsync(url);
    }
}
