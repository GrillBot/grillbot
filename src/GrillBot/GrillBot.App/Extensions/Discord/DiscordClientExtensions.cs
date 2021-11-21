using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        static public async Task<IUser> FindUserAsync(this BaseSocketClient client, ulong id)
        {
            var user = client.GetUser(id);

            if (user != null)
                return user;

            foreach (var guild in client.Guilds)
            {
                await guild.DownloadUsersAsync();

                user = guild.GetUser(id);

                if (user != null)
                    return user;
            }

            if (client.LoginState != LoginState.LoggedIn)
                return null;

            return await client.Rest.GetUserAsync(id);
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

        static public IRole FindRole(this BaseSocketClient client, ulong id)
        {
            return client.Guilds.SelectMany(o => o.Roles)
                .Where(o => !o.IsEveryone)
                .FirstOrDefault(o => o.Id == id);
        }

        static public IEnumerable<SocketGuild> FindMutualGuilds(this BaseSocketClient client, ulong userId)
        {
            return client.Guilds
                .Where(o => o.GetUser(userId) != null);
        }
    }
}