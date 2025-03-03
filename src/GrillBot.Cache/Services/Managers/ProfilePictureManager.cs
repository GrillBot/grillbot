using Discord;
using GrillBot.Cache.Models;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Core.Managers.Performance;
using Microsoft.Extensions.Caching.Distributed;

namespace GrillBot.Cache.Services.Managers;

public class ProfilePictureManager
{
    private readonly ICounterManager _counter;
    private readonly IDistributedCache _cache;

    private static readonly DistributedCacheEntryOptions _cacheOptions = new DistributedCacheEntryOptions()
        .SetAbsoluteExpiration(TimeSpan.FromDays(14.0D));

    public ProfilePictureManager(ICounterManager counterManager, IDistributedCache cache)
    {
        _counter = counterManager;
        _cache = cache;
    }

    public async Task<ProfilePicture> GetOrCreatePictureAsync(IUser user, ushort size = 128)
    {
        var avatarId = string.IsNullOrEmpty(user.AvatarId) ? user.Id.ToString() : user.AvatarId;
        var cacheKey = CreateCacheKey(user, avatarId, size);
        var profilePicture = await _cache.GetAsync(cacheKey);

        if (profilePicture is not null)
            return CreatePicture(user, avatarId, size, profilePicture);

        profilePicture = await DownloadAvatarAsync(user, size);
        await _cache.SetAsync(cacheKey, profilePicture, _cacheOptions);

        return CreatePicture(user, avatarId, size, profilePicture);
    }

    private static ProfilePicture CreatePicture(IUser user, string avatarId, ushort size, byte[] image)
    {
        return new ProfilePicture
        {
            AvatarId = avatarId,
            Data = image,
            IsAnimated = avatarId.StartsWith("a_"),
            Size = (short)size,
            UserId = user.Id
        };
    }

    private async Task<byte[]> DownloadAvatarAsync(IUser user, ushort size = 128)
    {
        using (_counter.Create("Discord.CDN"))
        {
            var url = user.GetUserAvatarUrl(size);

            using var httpClient = new HttpClient();
            return await httpClient.GetByteArrayAsync(url);
        }
    }

    private static string CreateCacheKey(IUser user, string avatarId, ushort size)
        => $"ProfilePicture-{user.Id}-{avatarId}-{size}";
}
