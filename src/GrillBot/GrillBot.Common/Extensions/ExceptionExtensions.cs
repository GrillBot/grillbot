using Discord;
using Commands = Discord.Commands;

namespace GrillBot.Common.Extensions;

public static class ExceptionExtensions
{
    public static IUser GetUser(this Exception exception, IDiscordClient client)
    {
        IUser? user = null;

        if (exception is Commands.CommandException commandException)
            user = commandException.Context?.User;

        return user ?? client.CurrentUser;
    }
}
