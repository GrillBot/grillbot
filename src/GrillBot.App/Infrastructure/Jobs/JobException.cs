namespace GrillBot.App.Infrastructure.Jobs;

public class JobException : Exception
{
    public IUser? LoggedUser { get; set; }

    public JobException()
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
