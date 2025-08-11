using GrillBot.Data.Models;
using GrillBot.Data.Models.Unverify;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;

namespace GrillBot.App.Managers;

public class UnverifyLogManager
{
    private IDiscordClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public UnverifyLogManager(IDiscordClient discordClient, GrillBotDatabaseBuilder databaseBuilder)
    {
        DiscordClient = discordClient;
        DatabaseBuilder = databaseBuilder;
    }

    public Task<UnverifyLog> LogUnverifyAsync(UnverifyUserProfile profile, IGuild guild, IGuildUser from)
    {
        var data = UnverifyLogSet.FromProfile(profile);
        return SaveAsync(UnverifyOperation.Unverify, data, from, guild, profile.Destination);
    }

    public Task<UnverifyLog> LogSelfunverifyAsync(UnverifyUserProfile profile, IGuild guild)
    {
        var data = UnverifyLogSet.FromProfile(profile);
        return SaveAsync(UnverifyOperation.Selfunverify, data, profile.Destination, guild, profile.Destination);
    }

    private async Task<UnverifyLog> SaveAsync(UnverifyOperation operation, object data, IGuildUser from, IGuild guild, IGuildUser toUser)
    {
        var entity = new UnverifyLog
        {
            CreatedAt = DateTime.Now,
            Data = JsonConvert.SerializeObject(data),
            FromUserId = from.Id.ToString(),
            GuildId = guild.Id.ToString(),
            Operation = operation,
            ToUserId = toUser.Id.ToString()
        };

        using var repository = DatabaseBuilder.CreateRepository();

        await repository.Guild.GetOrCreateGuildAsync(guild);
        await repository.User.GetOrCreateUserAsync(from);
        await repository.GuildUser.GetOrCreateGuildUserAsync(from);

        if (from != toUser)
        {
            await repository.User.GetOrCreateUserAsync(toUser);
            await repository.GuildUser.GetOrCreateGuildUserAsync(toUser);
        }

        await repository.AddAsync(entity);
        await repository.CommitAsync();

        return entity;
    }
}
