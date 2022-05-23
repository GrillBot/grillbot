using Discord;
using GrillBot.Cache.Entity;
using GrillBot.Common.Extensions.Discord;

namespace GrillBot.Cache.Services.Managers;

public class ProfilePictureManager
{
    private GrillBotCacheBuilder CacheBuilder { get; }

    public ProfilePictureManager(GrillBotCacheBuilder cacheBuilder)
    {
        CacheBuilder = cacheBuilder;
    }

    public async Task<ProfilePicture> GetOrCreatePictureAsync(IUser user, ushort size = 128)
    {
        using var cache = CacheBuilder.CreateRepository();

        var avatarId = string.IsNullOrEmpty(user.AvatarId) ? user.Discriminator : user.AvatarId;
        var profilePictures = await cache.ProfilePictureRepository.GetProfilePicturesAsync(user.Id, avatarId);
        var profilePicture = profilePictures.Find(o => o.Size == size);

        if (profilePicture == null)
        {
            await CleanCacheForUserAsync(user); // Remove all profile pictures if user changed picture.
            return await CreatePictureAsync(user, size);
        }

        return profilePicture;
    }

    public async Task<ProfilePicture> CreatePictureAsync(IUser user, ushort size = 128)
    {
        var avatarData = await user.DownloadAvatarAsync(size);
        var entity = new ProfilePicture()
        {
            AvatarId = string.IsNullOrEmpty(user.AvatarId) ? user.Discriminator : user.AvatarId,
            IsAnimated = user.AvatarId?.StartsWith("a_") ?? false,
            Size = (short)size,
            UserId = user.Id.ToString(),
            Data = avatarData
        };

        using var cache = CacheBuilder.CreateRepository();
        await cache.AddAsync(entity);
        await cache.CommitAsync();

        return entity;
    }

    private async Task CleanCacheForUserAsync(IUser user)
    {
        using var cache = CacheBuilder.CreateRepository();

        var avatarId = string.IsNullOrEmpty(user.AvatarId) ? user.Discriminator : user.AvatarId;
        var invalidProfilePictures = await cache.ProfilePictureRepository.GetProfilePicturesExceptOneAsync(user.Id, avatarId);
        if (invalidProfilePictures.Count > 0)
        {
            cache.RemoveCollection(invalidProfilePictures);
            await cache.CommitAsync();
        }
    }
}
