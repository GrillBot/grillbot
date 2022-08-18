using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

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

    public string GetStatusCode()
    {
        if (string.IsNullOrEmpty(StatusCode)) return null;
        
        // Valid format of status code.
        if (Regex.IsMatch(StatusCode, @"\d+\s+\([^\)]+\)"))
            return StatusCode;

        // Reformat status code to valid format.
        var statusCodeData = StatusCode.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return $"{statusCodeData[0]} ({string.Concat(statusCodeData.Skip(1))})";
    }

    public bool IsCorrupted()
    {
        return string.IsNullOrEmpty(StatusCode) || StartAt == DateTime.MinValue || string.IsNullOrEmpty(Method) || string.IsNullOrEmpty(TemplatePath);
    }
}
