using GrillBot.App.Extensions.Discord;
using GrillBot.App.Infrastructure;
using GrillBot.Data.Extensions;
using GrillBot.Database.Entity;

namespace GrillBot.App.Services.Birthday
{
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

            dbUser.Birthday = birthday;
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
            var users = await context.Users.AsQueryable()
                .Where(o => o.Birthday != null && o.Birthday.Value.Month == today.Month && o.Birthday.Value.Day == today.Day)
                .Select(o => new Database.Entity.User { Id = o.Id, Birthday = o.Birthday })
                .ToListAsync(cancellationToken);

            var result = new List<Tuple<IUser, int?>>();

            foreach (var entity in users)
            {
                var user = await DiscordClient.FindUserAsync(Convert.ToUInt64(entity.Id));

                if (user != null)
                    result.Add(new Tuple<IUser, int?>(user, entity.BirthdayAcceptYear ? entity.Birthday.Value.ComputeAge() : null));
            }

            return result;
        }
    }
}
