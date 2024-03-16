namespace GrillBot.Data.Models.API.Emotes;

public class EmoteItem
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public string FullId { get; set; } = null!;
}