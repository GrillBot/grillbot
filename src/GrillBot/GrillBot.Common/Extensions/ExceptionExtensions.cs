using System.ComponentModel.DataAnnotations;
using Discord;
using GrillBot.Common.Exceptions;
using Commands = Discord.Commands;

namespace GrillBot.Common.Extensions;

public static class ExceptionExtensions
{
    public static IUser GetUser(this Exception exception, IDiscordClient client)
    {
        var user = exception switch
        {
            Commands.CommandException commandException => commandException.Context?.User,
            ApiException apiException => apiException.LoggedUser,
            _ => null
        };

        return user ?? client.CurrentUser;
    }

    public static ValidationException ToBadRequestValidation(this ValidationException exception, object? value, params string[] memberNames)
        => new ValidationException(new ValidationResult(exception.Message, memberNames), null, value);
}
