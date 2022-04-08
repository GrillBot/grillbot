using GrillBot.Data.Models.API.Selfunverify;
using GrillBot.Database.Entity;

namespace GrillBot.App.Services.Unverify
{
    public class SelfunverifyService
    {
        public UnverifyService UnverifyService { get; }
        private GrillBotContextFactory DbFactory { get; }

        public SelfunverifyService(UnverifyService unverifyService, GrillBotContextFactory dbFactory)
        {
            UnverifyService = unverifyService;
            DbFactory = dbFactory;
        }

        public Task<string> ProcessSelfUnverifyAsync(SocketUser user, DateTime end, SocketGuild guild, List<string> toKeep)
        {
            var guildUser = user as SocketGuildUser ?? guild.GetUser(user.Id);
            return UnverifyService.SetUnverifyAsync(guildUser, end, null, guild, guildUser, true, toKeep, null, false);
        }

        public async Task AddKeepablesAsync(List<KeepableParams> parameters)
        {
            foreach (var param in parameters)
            {
                param.Group = param.Group.ToLower();
                param.Name = param.Name.ToLower();
            }

            using var context = DbFactory.Create();

            foreach (var param in parameters)
            {
                if (await context.SelfunverifyKeepables.AsNoTracking().AnyAsync(o => o.GroupName == param.Group && o.Name == param.Name))
                    throw new ValidationException($"Ponechatelná role, nebo kanál {param.Group}/{param.Name} již existuje.");
            }

            var entities = parameters.ConvertAll(o => new SelfunverifyKeepable() { Name = o.Name, GroupName = o.Group });
            await context.AddRangeAsync(entities);
            await context.SaveChangesAsync();
        }

        public async Task RemoveKeepableAsync(string group, string name = null)
        {
            var groupName = group.ToLower();
            var itemName = name?.ToLower();

            using var context = DbFactory.Create();
            var itemsQuery = context.SelfunverifyKeepables.AsQueryable().Where(o => o.GroupName == groupName);

            if (string.IsNullOrEmpty(itemName))
            {
                if (!await itemsQuery.AnyAsync())
                    throw new ValidationException($"Skupina ponechatelných rolí, nebo kanálů {group} neexistuje.");

                var items = await itemsQuery.ToListAsync();
                context.RemoveRange(items);
            }
            else
            {
                if (!await itemsQuery.AnyAsync(o => o.Name == itemName))
                    throw new ValidationException($"Ponechatelná role, nebo kanál {group}/{name} neexistuje.");

                var item = await itemsQuery.FirstOrDefaultAsync(o => o.Name == itemName);
                context.Remove(item);
            }

            await context.SaveChangesAsync();
        }

        public async Task<Dictionary<string, List<string>>> GetKeepablesAsync(string group, CancellationToken cancellationToken = default)
        {
            var groupName = group?.ToLower();

            using var context = DbFactory.Create();
            var query = context.SelfunverifyKeepables.AsQueryable()
                .OrderBy(o => o.GroupName).ThenBy(o => o.Name)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(groupName))
                query = query.Where(o => EF.Functions.ILike(o.GroupName, $"{groupName}%"));

            var data = await query.ToListAsync(cancellationToken);
            return data.GroupBy(o => o.GroupName.ToUpper())
                .ToDictionary(o => o.Key, o => o.Select(o => o.Name.ToUpper()).ToList());
        }

        public async Task<bool> KeepableExistsAsync(KeepableParams parameters, CancellationToken cancellationToken = default)
        {
            var groupName = parameters.Group.ToLower();
            var itemName = parameters.Name.ToLower();

            using var context = DbFactory.Create();
            return await context.SelfunverifyKeepables.AsNoTracking()
                .AnyAsync(o => o.GroupName == groupName && o.Name == itemName, cancellationToken);
        }
    }
}
