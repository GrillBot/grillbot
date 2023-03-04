namespace GrillBot.Common.Infrastructure;

public interface IApiObject
{
    Dictionary<string, string?> SerializeForLog();
}
