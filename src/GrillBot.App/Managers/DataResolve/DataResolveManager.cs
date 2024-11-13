using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics.CodeAnalysis;
using ApiModels = GrillBot.Data.Models.API;

namespace GrillBot.App.Managers.DataResolve;

public class DataResolveManager
{
    private readonly IDiscordClient _discordClient;
    private readonly GrillBotDatabaseBuilder _databaseBuilder;
    private readonly IMemoryCache _memoryCache;

    [MaybeNull] private GuildResolver _guildResolver;
    [MaybeNull] private GuildUserResolver _guildUserResolver;
    [MaybeNull] private ChannelResolver _channelResolver;
    [MaybeNull] private UserResolver _userResolver;
    [MaybeNull] private RoleResolver _roleResolver;

    public DataResolveManager(IDiscordClient discordClient, GrillBotDatabaseBuilder databaseBuilder,
        IMemoryCache memoryCache)
    {
        _discordClient = discordClient;
        _databaseBuilder = databaseBuilder;
        _memoryCache = memoryCache;
    }

    public async Task<ApiModels.Channels.Channel?> GetChannelAsync(ulong guildId, ulong channelId)
    {
        _channelResolver ??= new ChannelResolver(_discordClient, _databaseBuilder, _memoryCache);
        return await _channelResolver.GetChannelAsync(guildId, channelId);
    }

    public async Task<ApiModels.Guilds.Guild?> GetGuildAsync(ulong guildId)
    {
        _guildResolver ??= new GuildResolver(_discordClient, _databaseBuilder, _memoryCache);
        return await _guildResolver.GetGuildAsync(guildId);
    }

    public async Task<ApiModels.Users.GuildUser?> GetGuildUserAsync(ulong guildId, ulong userId)
    {
        _guildUserResolver ??= new GuildUserResolver(_discordClient, _databaseBuilder, _memoryCache);
        return await _guildUserResolver.GetGuildUserAsync(guildId, userId);
    }

    public async Task<ApiModels.Users.User?> GetUserAsync(ulong userId)
    {
        _userResolver ??= new UserResolver(_discordClient, _databaseBuilder, _memoryCache);
        return await _userResolver.GetUserAsync(userId);
    }

    public async Task<ApiModels.Role?> GetRoleAsync(ulong roleId)
    {
        _roleResolver ??= new RoleResolver(_discordClient, _databaseBuilder, _memoryCache);
        return await _roleResolver.GetRoleAsync(roleId);
    }
}
