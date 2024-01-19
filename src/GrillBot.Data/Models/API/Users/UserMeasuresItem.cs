using GrillBot.Data.Enums;
using System;

namespace GrillBot.Data.Models.API.Users;

public class UserMeasuresItem
{
    public UserMeasuresType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public User Moderator { get; set; } = null!;
    public string Reason { get; set; } = null!;
    public DateTime? ValidTo { get; set; }
}
