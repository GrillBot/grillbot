using GrillBot.Cache.Entity;
using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Cache.Services.Repository;

public class ProfilePictureRepository : SubRepositoryBase<GrillBotCacheContext>
{
    public ProfilePictureRepository(GrillBotCacheContext context, ICounterManager counter) : base(context, counter)
    {
    }

    public async Task<List<ProfilePicture>> GetProfilePicturesAsync(ulong userId, string? avatarId = null)
    {
        using (CreateCounter())
        {
            var query = Context.ProfilePictures
                .Where(o => o.UserId == userId.ToString());

            if (!string.IsNullOrEmpty(avatarId))
                query = query.Where(o => o.AvatarId == avatarId);

            return await query.ToListAsync();
        }
    }

    public async Task<List<ProfilePicture>> GetProfilePicturesExceptOneAsync(ulong userId, string avatarId)
    {
        using (CreateCounter())
        {
            return await Context.ProfilePictures
                .Where(o => o.UserId == userId.ToString() && o.AvatarId != avatarId)
                .ToListAsync();
        }
    }

    public async Task<List<ProfilePicture>> GetAllProfilePicturesAsync()
    {
        using (CreateCounter())
        {
            return await Context.ProfilePictures.ToListAsync();
        }
    }
}
