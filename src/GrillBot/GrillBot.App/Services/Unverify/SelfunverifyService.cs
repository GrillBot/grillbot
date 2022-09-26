using GrillBot.Data.Models.API.Selfunverify;

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

    public async Task<bool> KeepableExistsAsync(KeepableParams parameters)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        return await repository.SelfUnverify.KeepableExistsAsync(parameters.Group, parameters.Name);
    }
}
