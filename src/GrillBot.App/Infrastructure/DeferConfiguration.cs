namespace GrillBot.App.Infrastructure;

[AttributeUsage(AttributeTargets.Method)]
public class DeferConfiguration : Attribute
{
    /// <summary>
    /// This command require ephemeral processing.
    /// </summary>
    public bool RequireEphemeral { get; set; }
    
    /// <summary>
    /// This command solving defer under his own direction.
    /// </summary>
    public bool SuppressAuto { get; set; }
}
