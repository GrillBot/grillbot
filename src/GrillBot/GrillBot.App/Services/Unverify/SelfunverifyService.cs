using GrillBot.Data.Models.API.Selfunverify;
using GrillBot.Database.Entity;

namespace GrillBot.App.Services.Unverify;

public class SelfunverifyService
{
    private UnverifyService UnverifyService { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public SelfunverifyService(UnverifyService unverifyService, GrillBotDatabaseBuilder databaseBuilder)
    {
        UnverifyService = unverifyService;
        DatabaseBuilder = databaseBuilder;
    }

    public Task<string> ProcessSelfUnverifyAsync(SocketUser user, DateTime end, SocketGuild guild, List<string> toKeep, string locale)
    {
        var guildUser = user as SocketGuildUser ?? guild.GetUser(user.Id);
        return UnverifyService.SetUnverifyAsync(guildUser, end, null, guild, guildUser, true, toKeep, null, false, locale);
    }

    public async Task AddKeepablesAsync(List<KeepableParams> parameters)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        foreach (var param in parameters)
        {
            if (await repository.SelfUnverify.KeepableExistsAsync(param.Group, param.Name))
                throw new ValidationException($"Ponechatelná role nebo kanál {param.Group}/{param.Name} již existuje.");
        }

        var entities = parameters.ConvertAll(o => new SelfunverifyKeepable
        {
            Name = o.Name.ToLower(),
            GroupName = o.Group.ToLower()
        });

        await repository.AddCollectionAsync(entities);
        await repository.CommitAsync();
    }

    public async Task RemoveKeepableAsync(string group, string name = null)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        if (string.IsNullOrEmpty(name))
        {
            if (!await repository.SelfUnverify.KeepableExistsAsync(group))
                throw new ValidationException($"Skupina ponechatelných rolí nebo kanálů {group} neexistuje.");

            var items = await repository.SelfUnverify.GetKeepablesAsync(group);
            if (items.Count > 0)
                repository.RemoveCollection(items);
        }
        else
        {
            if (!await repository.SelfUnverify.KeepableExistsAsync(group, name))
                throw new ValidationException($"Ponechatelná role nebo kanál {group}/{name} neexistuje.");

            var item = await repository.SelfUnverify.FindKeepableAsync(group, name);
            if (item != null)
                repository.Remove(item);
        }

        await repository.CommitAsync();
    }

    public async Task<Dictionary<string, List<string>>> GetKeepablesAsync(string group)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var items = await repository.SelfUnverify.GetKeepablesAsync(group);
        return items.GroupBy(o => o.GroupName.ToUpper())
            .ToDictionary(o => o.Key, o => o.Select(x => x.Name.ToUpper()).ToList());
    }

    public async Task<bool> KeepableExistsAsync(KeepableParams parameters)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        return await repository.SelfUnverify.KeepableExistsAsync(parameters.Group, parameters.Name);
    }
}
