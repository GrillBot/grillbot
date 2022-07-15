namespace GrillBot.App.Infrastructure.OpenApi;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class OnlyDevelopmentAttribute : Attribute
{
}
