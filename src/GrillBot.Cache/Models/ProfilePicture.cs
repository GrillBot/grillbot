namespace GrillBot.Cache.Models;

public class ProfilePicture
{
    public ulong UserId { get; init; }
    public short Size { get; init; }
    public required string AvatarId { get; init; }
    public bool IsAnimated { get; init; }
    public required byte[] Data { get; init; }
}
