namespace GrillBot.App.Actions.Commands.Points.Chart;

[Flags]
public enum ChartsFilter
{
    None = 0,

    /// <summary>
    /// Only messages
    /// </summary>
    Messages = 1,

    /// <summary>
    /// Only reactions
    /// </summary>
    Reactions = 2,

    /// <summary>
    /// Only summary (Messages+Reactions)
    /// </summary>
    Summary = 4,

    /// <summary>
    /// Get all graphs
    /// </summary>
    All = Messages | Reactions | Summary
}
