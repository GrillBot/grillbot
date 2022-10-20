using Discord;

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

        if (user != null)
            return user;

        foreach (var guild in await client.GetGuildsAsync())
        {
            user = await guild.GetUserAsync(id);

            if (user != null)
                return user;
        }

        return user;
    }

    public static async Task<IGuildUser?> TryFindGuildUserAsync(this IDiscordClient client, ulong guildId, ulong userId)
    {
        var guild = await client.GetGuildAsync(guildId);
        if (guild == null) return null;

        return await guild.GetUserAsync(userId);
    }

    public static async Task<List<IRole>> GetRolesAsync(this IDiscordClient client)
    {
        return (await client.GetGuildsAsync())
            .SelectMany(o => o.Roles)
            .ToList();
    }

    public static async Task<List<IGuild>> FindMutualGuildsAsync(this IDiscordClient client, ulong userId)
    {
        var guilds = (await client.GetGuildsAsync()).ToList();

        return await guilds
            .FindAllAsync(async g => await g.GetUserAsync(userId) != null);
    }

    public static async Task<ITextChannel?> FindTextChannelAsync(this IDiscordClient client, ulong id)
    {
        foreach (var guild in await client.GetGuildsAsync())
        {
            var textChannel = await guild.GetTextChannelAsync(id);
            if (textChannel != null)
                return textChannel;
        }

        return null;
    }
}
