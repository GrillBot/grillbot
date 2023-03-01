namespace GrillBot.Data.Models.API.System;

public class DashboardService
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsAvailable { get; set; }

    public DashboardService()
    {
    }

    public DashboardService(string id, string name, bool isAvailable)
    {
        Id = id;
        Name = name;
        IsAvailable = isAvailable;
    }
}
