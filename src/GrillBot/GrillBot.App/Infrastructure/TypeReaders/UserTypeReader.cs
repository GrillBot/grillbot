using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GrillBot.App.Extensions.Discord;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrillBot.App.Infrastructure.TypeReaders
{
    public class UserTypeReader : TypeReader
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            // Match on caller
            if (Regex.IsMatch(input.Trim(), "^(me|j[a|á])$", RegexOptions.IgnoreCase))
                return TypeReaderResult.FromSuccess(context.User);

            // Match on users in DMs by username and discriminator or only username.
            IUser user;
            if (context.Guild == null && context.Client is BaseSocketClient client)
            {
                // DM's
                var discIndex = input.LastIndexOf("#");
                if (discIndex >= 0)
                {
                    var username = input.Substring(0, discIndex);

                    if (ushort.TryParse(input[(discIndex + 1)..], out ushort _))
                        user = await client.FindUserAsync(username, input[(discIndex + 1)..]);
                    else
                        user = await client.FindUserAsync(input, null);
                }
                else
                {
                    user = await client.FindUserAsync(input, null);
                }

                if (user != null)
                    return TypeReaderResult.FromSuccess(user);
            }
            else if (context.Guild is SocketGuild guild)
            {
                var matches = guild.Users
                     .Where(o => (!string.IsNullOrEmpty(o.Nickname) && o.Nickname.Contains(input, StringComparison.CurrentCultureIgnoreCase)) || o.Username.Contains(input, StringComparison.CurrentCultureIgnoreCase))
                     .Select(o => o as IGuildUser)
                     .Where(o => o != null)
                     .ToList();

                if (matches.Count == 1)
                    return TypeReaderResult.FromSuccess(matches[0]);

                // Finds user directly from discord and get guild user from memory.
                matches = (await guild.SearchUsersAsync(input)).Select(o => o as IGuildUser).Where(o => o != null).ToList();
                if (matches.Count == 1)
                {
                    user = guild.GetUser(matches[0].Id) ?? matches[0];
                    return TypeReaderResult.FromSuccess(user);
                }
            }

            var reader = new UserTypeReader<IUser>();
            return await reader.ReadAsync(context, input, services);
        }
    }
}
