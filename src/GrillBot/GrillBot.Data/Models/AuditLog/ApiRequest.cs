using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GrillBot.Data.Models.AuditLog;

public class ApiRequest
{
    public string ControllerName { get; set; }
    public string ActionName { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string Method { get; set; }
    public string TemplatePath { get; set; }
    public string Path { get; set; }
    public string LoggedUserRole { get; set; }
    public string StatusCode { get; set; }
    public Dictionary<string, string> QueryParams { get; set; } = new();
    public string BodyContent { get; set; }

    [OnSerializing]
    internal void OnSerializing(StreamingContext _)
    {
        if (QueryParams.Count == 0) QueryParams = null;
    }

    public void SetParams(object data)
        => BodyContent = JsonConvert.SerializeObject(data);
}
