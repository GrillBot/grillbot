﻿namespace GrillBot.Data.Models.API.Statistics;

public class UserActionCountItem
{
    public string Username { get; set; } = null!;
    public string Action { get; set; } = null!;
    public long Count { get; set; }
}
