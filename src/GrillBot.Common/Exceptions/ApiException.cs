using Discord;

namespace GrillBot.Common.Exceptions;

public class ApiException : Exception
{
    public IUser? LoggedUser { get; }
    public string? Path { get; }
    public string? ControllerInfo { get; }

    public ApiException()
    {
    }

    public ApiException(string message) : base(message)
    {
    }

    public ApiException(string message, Exception innerException, IUser? loggedUser, string path, string controllerInfo) : base(message, innerException)
    {
        LoggedUser = loggedUser;
        Path = path;
        ControllerInfo = controllerInfo;
    }

    public ApiException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
