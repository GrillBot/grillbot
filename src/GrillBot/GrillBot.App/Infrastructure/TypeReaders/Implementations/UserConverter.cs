using Discord.Commands;
using GrillBot.App.Extensions.Discord;
using System.Text.RegularExpressions;

namespace GrillBot.App.Infrastructure.TypeReaders.Implementations;

public class UserConverter : ConverterBase<IUser>
{
    public UserConverter(IServiceProvider provider, ICommandContext context) : base(provider, context)
    {
    }

    public UserConverter(IServiceProvider provider, IInteractionContext context) : base(provider, context)
    {
    }

    public override async Task<IUser> ConvertAsync(string value)
    {
        // Match on caller
        if (Regex.IsMatch(value.Trim(), "^(me|j[a|á])$", RegexOptions.IgnoreCase)) return User;

        // Match on users in DMs by username and discriminator or only username.
        IUser user;
        if (Guild == null && Client is BaseSocketClient client)
        {
            // DM's
            var discIndex = value.LastIndexOf("#");
            if (discIndex >= 0)
            {
                var username = value[..discIndex];

                if (ushort.TryParse(value[(discIndex + 1)..], out ushort _))
                    user = await client.FindUserAsync(username, value[(discIndex + 1)..]);
                else
                    user = await client.FindUserAsync(value, null);
            }
            else
            {
                user = await client.FindUserAsync(value, null);
            }

            if (user != null) return user;
        }
        else if (Guild is SocketGuild guild)
        {
            var matches = guild.Users
                 .Where(o => (!string.IsNullOrEmpty(o.Nickname) && o.Nickname.Contains(value, StringComparison.CurrentCultureIgnoreCase)) || o.Username.Contains(value, StringComparison.CurrentCultureIgnoreCase))
                 .Select(o => o as IGuildUser)
                 .Where(o => o != null)
                 .ToList();

            if (matches.Count == 1) return matches[0];

            // Finds user directly from discord and get guild user from memory.
            matches = (await guild.SearchUsersAsync(value)).Select(o => o as IGuildUser).Where(o => o != null).ToList();
            if (matches.Count == 1)
                return guild.GetUser(matches[0].Id) ?? matches[0];
        }

        return null;
    }
}
