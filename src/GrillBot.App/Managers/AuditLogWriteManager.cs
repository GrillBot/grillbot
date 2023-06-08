namespace GrillBot.App.Managers;

public static class AuditLogWriteManager
{
    public static JsonSerializerSettings SerializerSettings => new()
    {
        DefaultValueHandling = DefaultValueHandling.Ignore,
        Formatting = Formatting.None,
        NullValueHandling = NullValueHandling.Ignore
    };
}
