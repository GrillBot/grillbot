namespace GrillBot.Data.Models.API.System;

public class DashboardCommand
{
    public string CommandName { get; set; } = null!;
    public int Duration { get; set; }
    public bool Success { get; set; }
}
