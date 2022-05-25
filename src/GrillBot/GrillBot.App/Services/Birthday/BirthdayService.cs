using GrillBot.App.Infrastructure;
using GrillBot.Common.Extensions;
using GrillBot.Data.Extensions;

namespace GrillBot.App.Services.Birthday;

public class BirthdayService : ServiceBase
{
    public BirthdayService(DiscordSocketClient client, GrillBotContextFactory dbFactory) : base(client, dbFactory)
    {
    }

    public async Task AddBirthdayAsync(IUser user, DateTime birthday)
    {
        using var context = DbFactory.Create();

        var dbUser = await context.Users.AsQueryable()
            .FirstOrDefaultAsync(o => o.Id == user.Id.ToString());

        if (dbUser == null)
        {
            dbUser = Database.Entity.User.FromDiscord(user);
            await context.AddAsync(dbUser);
        }

        dbUser.Birthday = birthday.Date;
        await context.SaveChangesAsync();
    }

    public async Task RemoveBirthdayAsync(IUser user)
    {
        using var context = DbFactory.Create();

        var dbUser = await context.Users.AsQueryable()
            .FirstOrDefaultAsync(o => o.Id == user.Id.ToString());

        if (dbUser == null) return;
        dbUser.Birthday = null;
        await context.SaveChangesAsync();
    }

    public async Task<bool> HaveBirthdayAsync(IUser user)
    {
        using var context = DbFactory.Create();
        return await context.Users.AsQueryable()
            .Where(o => o.Id == user.Id.ToString())
            .Select(o => o.Birthday != null)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Tuple<IUser, int?>>> GetTodayBirthdaysAsync(CancellationToken cancellationToken = default)
    {
        using var context = DbFactory.Create();

        var today = DateTime.Today.Date;
        var query = context.Users.AsQueryable()
            .Where(o => o.Birthday != null && o.Birthday.Value.Month == today.Month && o.Birthday.Value.Day == today.Day)
            .Select(o => new Database.Entity.User { Id = o.Id, Birthday = o.Birthday });

        var users = await query.ToListAsync(cancellationToken);
        var result = new List<Tuple<IUser, int?>>();

        foreach (var entity in users)
        {
            var user = await DiscordClient.FindUserAsync(entity.Id.ToUlong(), cancellationToken);

            if (user != null)
                result.Add(new Tuple<IUser, int?>(user, entity.BirthdayAcceptYear ? entity.Birthday.Value.ComputeAge() : null));
        }

        return result;
    }
}
