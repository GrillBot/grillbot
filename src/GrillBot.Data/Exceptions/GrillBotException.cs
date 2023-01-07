using System;
using System.Runtime.Serialization;

namespace GrillBot.Data.Exceptions;

[Serializable]
public class GrillBotException : Exception
{
    public GrillBotException()
    {
    }

    public GrillBotException(string message) : base(message)
    {
    }

    public GrillBotException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected GrillBotException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
