using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GrillBot.App.Extensions.Discord;
using System;
using System.Threading.Tasks;

namespace GrillBot.App.Infrastructure.TypeReaders
{
    public class UserTypeReader : TypeReader
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (string.Equals(input.Trim(), "me", StringComparison.OrdinalIgnoreCase))
                return TypeReaderResult.FromSuccess(context.User);

            if (context.Guild == null && context.Client is BaseSocketClient client)
            {
                // DM's
                var discIndex = input.LastIndexOf("#");
                IUser user;
                if (discIndex >= 0)
                {
                    var username = input.Substring(0, discIndex);

                    if (ushort.TryParse(input[(discIndex + 1)..], out ushort _))
                        user = await client.FindUserAsync(username, input[(discIndex + 1)..]);
                    else
                        user = await client.FindUserAsync(input);
                }
                else
                {
                    user = await client.FindUserAsync(input);
                }

                if (user != null)
                    return TypeReaderResult.FromSuccess(user);
            }

            var reader = new UserTypeReader<IUser>();
            return await reader.ReadAsync(context, input, services);
        }
    }
}
