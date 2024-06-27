using Discord;
using System.Runtime.Serialization;

namespace GrillBot.Common.Exceptions;

[Serializable]
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

    protected FrontendException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public FrontendException(string? message) : base(message)
    {
    }
}
