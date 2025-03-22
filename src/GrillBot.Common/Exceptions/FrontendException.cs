using Discord;

namespace GrillBot.Common.Exceptions;

public class FrontendException : Exception
{
    public IUser LoggedUser { get; } = null!;

    public FrontendException()
    {
    }

    public FrontendException(string? message, IUser user) : base(message)
    {
        LoggedUser = user;
    }

    public FrontendException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public FrontendException(string? message) : base(message)
    {
    }
}
