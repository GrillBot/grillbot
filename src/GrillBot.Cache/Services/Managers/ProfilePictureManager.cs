using Discord;
using GrillBot.Cache.Entity;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Counters;

namespace GrillBot.Cache.Services.Managers;

public class ProfilePictureManager
{
    private GrillBotCacheBuilder CacheBuilder { get; }
    private CounterManager Counter { get; }

    public ProfilePictureManager(GrillBotCacheBuilder cacheBuilder, CounterManager counterManager)
    {
        CacheBuilder = cacheBuilder;
        Counter = counterManager;
    }

    public async Task<ProfilePicture> GetOrCreatePictureAsync(IUser user, ushort size = 128)
    {
        await using var cache = CacheBuilder.CreateRepository();

        var avatarId = string.IsNullOrEmpty(user.AvatarId) ? user.Discriminator : user.AvatarId;
        var profilePictures = await cache.ProfilePictureRepository.GetProfilePicturesAsync(user.Id, avatarId);
        var profilePicture = profilePictures.Find(o => o.Size == size);

        if (profilePicture != null)
            return profilePicture;

        await CleanCacheForUserAsync(user); // Remove all profile pictures if user changed picture.
        return await CreatePictureAsync(user, size);
    }

    private async Task<ProfilePicture> CreatePictureAsync(IUser user, ushort size = 128)
    {
        var avatarData = await DownloadAvatarAsync(user, size);
        var entity = new ProfilePicture
        {
            AvatarId = string.IsNullOrEmpty(user.AvatarId) ? user.Discriminator : user.AvatarId,
            IsAnimated = user.AvatarId?.StartsWith("a_") ?? false,
            Size = (short)size,
            UserId = user.Id.ToString(),
            Data = avatarData
        };

        await using var cache = CacheBuilder.CreateRepository();
        await cache.AddAsync(entity);
        await cache.CommitAsync();

        return entity;
    }

    private async Task<byte[]> DownloadAvatarAsync(IUser user, ushort size = 128)
    {
        using (Counter.Create("Discord.CDN"))
        {
            var url = user.GetUserAvatarUrl(size);

            using var httpClient = new HttpClient();
            return await httpClient.GetByteArrayAsync(url);
        }
    }

    private async Task CleanCacheForUserAsync(IUser user)
    {
        await using var cache = CacheBuilder.CreateRepository();

        var avatarId = string.IsNullOrEmpty(user.AvatarId) ? user.Discriminator : user.AvatarId;
        var invalidProfilePictures = await cache.ProfilePictureRepository.GetProfilePicturesExceptOneAsync(user.Id, avatarId);
        if (invalidProfilePictures.Count == 0) return;

        cache.RemoveCollection(invalidProfilePictures);
        await cache.CommitAsync();
    }
}
