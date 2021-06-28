using Discord;
using Discord.WebSocket;
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

            return await client.Rest.GetUserAsync(id);
        }
    }
}
