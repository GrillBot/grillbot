namespace GrillBot.Data.Models.API.Users;

public class KarmaListItem
{
    public User User { get; set; } = null!;
    public int Negative { get; set; }
    public int Positive { get; set; }
    public int Value { get; set; }
    public int Position { get; set; }
}
