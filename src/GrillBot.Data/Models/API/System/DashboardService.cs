namespace GrillBot.Data.Models.API.System;

public class DashboardService
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsAvailable { get; set; }
    public long Uptime { get; set; }

    public DashboardService()
    {
    }

    public DashboardService(string id, string name, bool isAvailable, long uptime)
    {
        Id = id;
        Name = name;
        IsAvailable = isAvailable;
        Uptime = uptime;
    }
}
