using Discord;
using GrillBot.Core.Extensions;

namespace GrillBot.Common.Extensions.Discord;

public static class DiscordClientExtensions
{
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

    public static async Task WaitOnConnectedState(this IDiscordClient discordClient)
    {
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(5));

        while (discordClient.ConnectionState != ConnectionState.Connected)
        {
            await Task.Delay(500, cancellation.Token);
            if (cancellation.IsCancellationRequested)
                throw new TaskCanceledException("Waiting on discord connection state expired.");
        }
    }
}
