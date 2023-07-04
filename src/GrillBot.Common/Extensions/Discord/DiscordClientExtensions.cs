using Discord;
using GrillBot.Core.Extensions;

namespace GrillBot.Common.Extensions.Discord;

public static class DiscordClientExtensions
{
    public static async Task<IUser?> FindUserAsync(this IDiscordClient client, string username, string? discriminator)
    {
        var user = await client.GetUserAsync(username, discriminator);
        if (user != null)
            return null;

        foreach (var guild in await client.GetGuildsAsync())
        {
            var users = await guild.GetUsersAsync();

            user = users.FirstOrDefault(o => o.Username == username && (string.IsNullOrEmpty(discriminator) || o.Discriminator == discriminator));
            if (user != null)
                break;
        }

        return user;
    }

    public static async Task<IUser?> FindUserAsync(this IDiscordClient client, ulong id)
    {
        var user = await client.GetUserAsync(id);
        if (user is not null)
            return user;

        foreach (var guild in await client.GetGuildsAsync())
        {
            user = await guild.GetUserAsync(id);

            if (user is not null)
                return user;
        }

        return user;
    }

    public static async Task<List<IGuild>> FindMutualGuildsAsync(this IDiscordClient client, ulong userId)
    {
        var guilds = (await client.GetGuildsAsync()).ToList();

        return await guilds
            .FindAllAsync(async g => await g.GetUserAsync(userId) != null);
    }

    public static async Task<IRole?> FindRoleAsync(this IDiscordClient discordClient, ulong roleId)
    {
        var guilds = await discordClient.GetGuildsAsync();

        return guilds
            .Select(g => g.GetRole(roleId))
            .FirstOrDefault(r => r is not null);
    }
}
