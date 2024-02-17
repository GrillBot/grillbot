using System.ComponentModel.DataAnnotations;
using Discord;
using Discord.Interactions;
using GrillBot.Common.Exceptions;

namespace GrillBot.Common.Extensions;

public static class ExceptionExtensions
{
    public static IUser? GetUser(this Exception exception)
    {
        return exception switch
        {
            ApiException apiException => apiException.LoggedUser,
            InteractionException interactionException => interactionException.InteractionContext.User,
            _ => null
        };
    }

    public static ValidationException ToBadRequestValidation(this ValidationException exception, object? value, params string[] memberNames) =>
        new(new ValidationResult(exception.Message, memberNames), null, value);
}
