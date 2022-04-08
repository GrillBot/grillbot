namespace GrillBot.App.Infrastructure.Jobs;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DisallowUninitializedAttribute : Attribute
{
}
