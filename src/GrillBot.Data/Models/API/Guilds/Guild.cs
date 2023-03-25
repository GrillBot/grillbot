namespace GrillBot.Data.Models.API.Guilds;

/// <summary>
/// Simple guild item.
/// </summary>
public class Guild
{
    /// <summary>
    /// Guild ID.
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// Name of guild.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Member count.
    /// </summary>
    public int MemberCount { get; set; }

    /// <summary>
    /// Flag that describe information about connection state.
    /// </summary>
    public bool IsConnected { get; set; }
}
