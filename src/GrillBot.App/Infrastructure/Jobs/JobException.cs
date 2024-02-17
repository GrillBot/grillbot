using System.Runtime.Serialization;

namespace GrillBot.App.Infrastructure.Jobs;

[Serializable]
public class JobException : Exception
{
    public IUser? LoggedUser { get; set; }

    public JobException()
    {
    }

    protected JobException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public JobException(string? message) : base(message)
    {
    }

    public JobException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public JobException(IUser? loggedUser, Exception? innerException) : base(null, innerException)
    {
        LoggedUser = loggedUser;
    }
}
