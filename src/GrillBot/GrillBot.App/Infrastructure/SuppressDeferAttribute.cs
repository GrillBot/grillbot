namespace GrillBot.App.Infrastructure;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class SuppressDeferAttribute : Attribute
{
}
