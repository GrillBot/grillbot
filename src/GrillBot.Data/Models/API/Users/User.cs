namespace GrillBot.Data.Models.API.Users;

public class User
{
    /// <summary>
    /// Discord ID of user.
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// Username.
    /// </summary>
    public string Username { get; set; } = null!;

    public string? GlobalAlias { get; set; }

    /// <summary>
    /// Flag that describe user is bot.
    /// </summary>
    public bool IsBot { get; set; }

    /// <summary>
    /// Avatar url.
    /// </summary>
    public string AvatarUrl { get; set; } = null!;

    [global::System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string DisplayName
        => string.IsNullOrEmpty(GlobalAlias) ? Username : $"{Username} ({GlobalAlias})";
}
