using GrillBot.Data.Enums;
using GrillBot.Data.Models.API.Users;
using System;

namespace GrillBot.Data.Models.API.UserMeasures;

public class UserMeasuresItem
{
    public UserMeasuresType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public User Moderator { get; set; } = null!;
    public string Reason { get; set; } = null!;
    public DateTime? ValidTo { get; set; }
}
