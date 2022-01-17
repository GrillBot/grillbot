using GrillBot.Data.Extensions;
using System;

namespace GrillBot.Data.Models;

public class EmoteStatItem
{
    public string Id { get; set; }
    public int UsersCount { get; set; }
    public long UseCount { get; set; }
    public DateTime FirstOccurence { get; set; }
    public DateTime LastOccurence { get; set; }

    public override string ToString()
    {
        return string.Join("\n", new[]
        {
            $"Počet použití: **{UseCount}**",
            $"Použilo uživatelů: **{UsersCount}**",
            $"První použití: **{FirstOccurence.ToCzechFormat()}**",
            $"Poslední použití: **{LastOccurence.ToCzechFormat()}**"
        });
    }
}
