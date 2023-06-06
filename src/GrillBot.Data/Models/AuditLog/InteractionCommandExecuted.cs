using Discord.Interactions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GrillBot.Data.Models.AuditLog;

public class InteractionCommandExecuted
{
    public string Name { get; set; }
    public string ModuleName { get; set; }
    public string MethodName { get; set; }
    public List<InteractionCommandParameter>? Parameters { get; set; }
    public bool HasResponded { get; set; }
    public bool IsValidToken { get; set; }
    public bool IsSuccess { get; set; }
    public InteractionCommandError? CommandError { get; set; }
    public string ErrorReason { get; set; }
    public int Duration { get; set; }
    public string Exception { get; set; }
    public string Locale { get; set; }

    [JsonIgnore]
    public string FullName => $"{Name} ({ModuleName}/{MethodName})";

    [OnSerializing]
    internal void OnSerializing(StreamingContext _)
    {
        if (Parameters?.Count == 0) Parameters = null;
    }
}
