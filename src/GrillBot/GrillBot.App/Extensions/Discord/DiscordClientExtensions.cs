using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace GrillBot.App.Extensions.Discord
{
    static public class DiscordClientExtensions
    {
        // TODO: Tests
        static public async Task<IUser> FindUserAsync(this BaseSocketClient client, ulong id)
        {
            var user = client.GetUser(id);

            if (user != null)
                return user;

            foreach(var guild in client.Guilds)
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
