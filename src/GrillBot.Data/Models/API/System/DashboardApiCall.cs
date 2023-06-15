namespace GrillBot.Data.Models.API.System;

public class DashboardApiCall
{
    public string Endpoint { get; set; } = null!;
    public long Duration { get; set; }
    public string StatusCode { get; set; } = null!;
}
