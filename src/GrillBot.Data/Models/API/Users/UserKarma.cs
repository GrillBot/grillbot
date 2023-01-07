namespace GrillBot.Data.Models.API.Users;

public class UserKarma
{
    public User User { get; set; }

    public int Position { get; set; }
    public int Positive { get; set; }
    public int Negative { get; set; }
    public int Value { get; set; }
}
