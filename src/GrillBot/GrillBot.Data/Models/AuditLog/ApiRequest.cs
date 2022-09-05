using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using GrillBot.Common.Infrastructure;

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
    public Dictionary<string, string> Parameters { get; set; } = new();

    [OnSerializing]
    internal void OnSerializing(StreamingContext _)
    {
        if (Parameters?.Count == 0) Parameters = null;
    }

    public void AddParameters(IApiObject apiObject, int index = -1)
    {
        if (apiObject == null) return;

        foreach (var item in apiObject.SerializeForLog())
            AddParameter((index == -1 ? "" : index + ".") + item.Key, item.Value);
    }

    public void AddParameters(IApiObject[] apiObjects)
    {
        for (var i = 0; i < apiObjects.Length; i++)
            AddParameters(apiObjects[i], i);
    }

    public void AddParameter(string id, string value)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(value))
            return;
        Parameters[id] = value;
    }

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
