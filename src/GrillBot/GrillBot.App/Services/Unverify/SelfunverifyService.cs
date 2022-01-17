using Discord.WebSocket;
using GrillBot.Database.Entity;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.Data.Services.Unverify
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

        public async Task AddKeepableAsync(string group, string name)
        {
            var groupName = group.ToLower();
            var itemName = name.ToLower();

            using var context = DbFactory.Create();

            if (await context.SelfunverifyKeepables.AsQueryable().AnyAsync(o => o.GroupName == groupName && o.Name == itemName))
                throw new ValidationException($"Ponechatelná role, nebo kanál {group}/{name} již existuje.");

            var entity = new SelfunverifyKeepable()
            {
                GroupName = groupName,
                Name = itemName
            };

            await context.AddAsync(entity);
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

        public async Task<Dictionary<string, List<string>>> GetKeepablesAsync(string group)
        {
            var groupName = group?.ToLower();

            using var context = DbFactory.Create();
            var query = context.SelfunverifyKeepables.AsQueryable()
                .OrderBy(o => o.GroupName).ThenBy(o => o.Name)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(groupName))
                query = query.Where(o => EF.Functions.ILike(o.GroupName, $"{groupName}%"));

            var data = await query.ToListAsync();
            return data.GroupBy(o => o.GroupName.ToUpper())
                .ToDictionary(o => o.Key, o => o.Select(o => o.Name.ToUpper()).ToList());
        }
    }
}
