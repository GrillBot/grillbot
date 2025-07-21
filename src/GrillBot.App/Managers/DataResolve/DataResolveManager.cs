using GrillBot.Cache.Services.Managers;
using System.Diagnostics.CodeAnalysis;
using ApiModels = GrillBot.Data.Models.API;

namespace GrillBot.App.Managers.DataResolve;

public class DataResolveManager
{
    private readonly IDiscordClient _discordClient;
    private readonly GrillBotDatabaseBuilder _databaseBuilder;
    private readonly DataCacheManager _cache;

    [MaybeNull] private GuildResolver _guildResolver;
    [MaybeNull] private GuildUserResolver _guildUserResolver;
    [MaybeNull] private ChannelResolver _channelResolver;
    [MaybeNull] private UserResolver _userResolver;
    [MaybeNull] private RoleResolver _roleResolver;

    public DataResolveManager(IDiscordClient discordClient, GrillBotDatabaseBuilder databaseBuilder,
        DataCacheManager cache)
    {
        _discordClient = discordClient;
        _databaseBuilder = databaseBuilder;
        _cache = cache;
    }

    public async Task<ApiModels.Channels.Channel?> GetChannelAsync(ulong guildId, ulong channelId)
    {
        _channelResolver ??= new ChannelResolver(_discordClient, _databaseBuilder, _cache);
        return await _channelResolver.GetChannelAsync(guildId, channelId);
    }

    public async Task<ApiModels.Channels.Channel?> GetChannelAsync(ulong channelId)
    {
        _channelResolver ??= new ChannelResolver(_discordClient, _databaseBuilder, _cache);
        return await _channelResolver.GetChannelAsync(channelId);
    }

    public async Task<ApiModels.Guilds.Guild?> GetGuildAsync(ulong guildId)
    {
        _guildResolver ??= new GuildResolver(_discordClient, _databaseBuilder, _cache);
        return await _guildResolver.GetGuildAsync(guildId);
    }

    public async Task<ApiModels.Users.GuildUser?> GetGuildUserAsync(ulong guildId, ulong userId)
    {
        _guildUserResolver ??= new GuildUserResolver(_discordClient, _databaseBuilder, _cache);
        return await _guildUserResolver.GetGuildUserAsync(guildId, userId);
    }

    public async Task<ApiModels.Users.User?> GetUserAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        _userResolver ??= new UserResolver(_discordClient, _databaseBuilder, _cache);
        return await _userResolver.GetUserAsync(userId, cancellationToken);
    }

    public async Task<ApiModels.Role?> GetRoleAsync(ulong roleId)
    {
        _roleResolver ??= new RoleResolver(_discordClient, _databaseBuilder, _cache);
        return await _roleResolver.GetRoleAsync(roleId);
    }
}
