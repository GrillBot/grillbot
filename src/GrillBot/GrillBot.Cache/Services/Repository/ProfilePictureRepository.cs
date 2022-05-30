using GrillBot.Cache.Entity;
using GrillBot.Common.Managers.Counters;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Cache.Services.Repository;

public class ProfilePictureRepository : RepositoryBase
{
    public ProfilePictureRepository(GrillBotCacheContext context, CounterManager counter) : base(context, counter)
    {
    }

    public List<ProfilePicture> GetAll() => Context.ProfilePictures.ToList();

    public async Task<List<ProfilePicture>> GetProfilePicturesAsync(ulong userId, string? avatarId = null)
    {
        using (Counter.Create("Cache"))
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
        using (Counter.Create("Cache"))
        {
            return await Context.ProfilePictures
                .Where(o => o.UserId == userId.ToString() && o.AvatarId != avatarId)
                .ToListAsync();
        }
    }
}
