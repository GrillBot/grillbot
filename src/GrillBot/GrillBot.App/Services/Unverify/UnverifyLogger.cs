using GrillBot.Data.Models;
using GrillBot.Data.Models.Unverify;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.Unverify;

public class UnverifyLogger
{
    private IDiscordClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public UnverifyLogger(IDiscordClient discordClient, GrillBotDatabaseBuilder databaseBuilder)
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

    public async Task LogAutoremoveAsync(List<IRole> returnedRoles, List<ChannelOverride> returnedChannels, IGuildUser toUser, IGuild guild, string language)
    {
        var data = new UnverifyLogRemove
        {
            ReturnedOverwrites = returnedChannels,
            ReturnedRoles = returnedRoles.ConvertAll(o => o.Id),
            Language = language,
            Force = false
        };

        var currentUser = await guild.GetUserAsync(DiscordClient.CurrentUser.Id);
        await SaveAsync(UnverifyOperation.Autoremove, data, currentUser, guild, toUser);
    }

    public Task LogRemoveAsync(List<IRole> returnedRoles, List<ChannelOverride> returnedChannels, IGuild guild, IGuildUser from, IGuildUser to, bool fromWeb, bool force, string language)
    {
        var data = new UnverifyLogRemove
        {
            ReturnedOverwrites = returnedChannels,
            ReturnedRoles = returnedRoles.ConvertAll(o => o.Id),
            FromWeb = fromWeb,
            Language = language,
            Force = force
        };

        return SaveAsync(UnverifyOperation.Remove, data, from, guild, to);
    }

    public Task LogUpdateAsync(DateTime start, DateTime end, IGuild guild, IGuildUser from, IGuildUser to, string reason)
    {
        var data = new UnverifyLogUpdate
        {
            End = end,
            Start = start,
            Reason = reason
        };

        return SaveAsync(UnverifyOperation.Update, data, from, guild, to);
    }

    public Task LogRecoverAsync(List<IRole> returnedRoles, List<ChannelOverride> returnedChannels, IGuild guild, IGuildUser from, IGuildUser to)
    {
        var data = new UnverifyLogRemove
        {
            ReturnedOverwrites = returnedChannels,
            ReturnedRoles = returnedRoles.ConvertAll(o => o.Id)
        };

        return SaveAsync(UnverifyOperation.Recover, data, from, guild, to);
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

        await using var repository = DatabaseBuilder.CreateRepository();

        await repository.GuildUser.GetOrCreateGuildUserAsync(from);
        if (from != toUser)
            await repository.GuildUser.GetOrCreateGuildUserAsync(toUser);

        await repository.AddAsync(entity);
        await repository.CommitAsync();

        return entity;
    }
}
