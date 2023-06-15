namespace GrillBot.Data.Models.API.System;

public class DashboardJob
{
    public string Name { get; set; } = null!;
    public int Duration { get; set; }
    public bool Success { get; set; }
}
