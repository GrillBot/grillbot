using GrillBot.Data.Extensions;

namespace GrillBot.App.Extensions.Discord
{
    static public class DiscordClientExtensions
    {
        static public async Task<IUser> FindUserAsync(this BaseSocketClient client, string username, string discriminator)
        {
            var user = client.GetUser(username, discriminator);

            if (user != null)
                return user;

            foreach (var guild in client.Guilds)
            {
                await guild.DownloadUsersAsync();
                user = guild.Users
                    .FirstOrDefault(o => o.Username == username && (string.IsNullOrEmpty(discriminator) || o.Discriminator == discriminator));

                if (user != null)
                    break;
            }

            return user;
        }

        static public async Task<IUser> FindUserAsync(this BaseSocketClient client, ulong id, CancellationToken cancellationToken = default)
        {
            var user = client.GetUser(id);

            if (user != null)
                return user;

            foreach (var guild in client.Guilds)
            {
                user = guild.GetUser(id);

                if (user != null)
                    return user;
            }

            if (client.LoginState != LoginState.LoggedIn)
                return null;

            return await client.Rest.GetUserAsync(id, new RequestOptions() { CancelToken = cancellationToken });
        }

        static public async Task<IUser> FindUserAsync(this IDiscordClient client, ulong id, CancellationToken cancellationToken = default)
        {
            var requestOptions = new RequestOptions() { CancelToken = cancellationToken };
            var user = await client.GetUserAsync(id, options: requestOptions);

            if (user != null)
                return user;

            foreach (var guild in await client.GetGuildsAsync(options: requestOptions))
            {
                user = await guild.GetUserAsync(id, options: requestOptions);

                if (user != null)
                    return user;
            }

            return user;
        }

        static public async Task<IGuildUser> TryFindGuildUserAsync(this BaseSocketClient client, ulong guildId, ulong userId)
        {
            var guild = client.GetGuild(guildId);
            if (guild == null) return null;

            IGuildUser user = guild.GetUser(userId);
            if (user == null)
            {
                var restGuild = await client.Rest.GetGuildAsync(guildId);
                user = await restGuild.GetUserAsync(userId);
            }

            return user;
        }

        static public IRole FindRole(this BaseSocketClient client, string id)
            => FindRole(client, Convert.ToUInt64(id));

        static public IRole FindRole(this BaseSocketClient client, ulong id)
        {
            return client.Guilds.SelectMany(o => o.Roles)
                .FirstOrDefault(o => !o.IsEveryone && o.Id == id);
        }

        static public async Task<List<IGuild>> FindMutualGuildsAsync(this IDiscordClient client, ulong userId)
        {
            var guilds = (await client.GetGuildsAsync()).ToList();

            return await guilds
                .FindAllAsync(async g => (await g.GetUserAsync(userId)) != null);
        }

        static public IEnumerable<SocketGuild> FindMutualGuilds(this BaseSocketClient client, ulong userId)
        {
            return client.Guilds
                .Where(o => o.GetUser(userId) != null);
        }

        static public ITextChannel FindTextChannel(this BaseSocketClient client, ulong channelId)
        {
            return client.Guilds
                .SelectMany(o => o.TextChannels)
                .FirstOrDefault(o => o.Id == channelId);
        }
    }
}