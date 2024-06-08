using GrillBot.Data.Models.API.Users;
using System;

namespace GrillBot.Data.Models.API.Reminder;

public class RemindMessage
{
    public long Id { get; set; }
    public User FromUser { get; set; } = null!;
    public User ToUser { get; set; } = null!;
    public DateTime At { get; set; }
    public string Message { get; set; } = null!;
    public int Postpone { get; set; }
    public bool Notified { get; set; }
    public string Language { get; set; } = null!;
}
