namespace GrillBot.Data.Models.API.Statistics;

public class UserActionCountItem
{
    public string Username { get; set; } = null!;
    public string Action { get; set; } = null!;
    public int Count { get; set; }

    public UserActionCountItem()
    {
    }

    public UserActionCountItem(string username, string action, int count)
    {
        Username = username;
        Action = action;
        Count = count;
    }
}
