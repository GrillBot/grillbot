using GrillBot.Cache.Entity;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Cache.Services.Repository;

public class ProfilePictureRepository : RepositoryBase
{
    public ProfilePictureRepository(GrillBotCacheContext context) : base(context)
    {
    }

    public List<ProfilePicture> GetAll() => Context.ProfilePictures.ToList();

    public async Task<List<ProfilePicture>> GetProfilePicturesAsync(ulong userId, string? avatarId = null)
    {
        var query = Context.ProfilePictures
            .Where(o => o.UserId == userId.ToString());

        if (!string.IsNullOrEmpty(avatarId))
            query = query.Where(o => o.AvatarId == avatarId);

        return await query.ToListAsync();
    }

    public async Task<List<ProfilePicture>> GetProfilePicturesExceptOneAsync(ulong userId, string avatarId)
    {
        return await Context.ProfilePictures
            .Where(o => o.UserId == userId.ToString() && o.AvatarId != avatarId)
            .ToListAsync();
    }
}
