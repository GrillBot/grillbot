using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using GrillBot.Common.Infrastructure;
using Microsoft.Extensions.Primitives;

namespace GrillBot.Data.Models.AuditLog;

public partial class ApiRequest
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
    public string Language { get; set; }
    public string ApiGroupName { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public string UserIdentification { get; set; }
    public string IpAddress { get; set; }

    [OnSerializing]
    internal void OnSerializing(StreamingContext _)
    {
        if (Parameters?.Count == 0) Parameters = null;
        if (Headers?.Count == 0) Headers = null;
    }

    public void AddParameters(IApiObject apiObject, int index = -1)
    {
        if (apiObject == null) return;

        foreach (var item in apiObject.SerializeForLog())
            AddParameter((index == -1 ? "" : index + ".") + item.Key, item.Value);
    }

    public void AddParameters(IEnumerable<IApiObject> apiObjects)
    {
        var objects = apiObjects.ToArray();
        for (var i = 0; i < objects.Length; i++)
            AddParameters(objects[i], i);
    }

    public void AddParameter(string id, string value)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(value))
            return;
        Parameters[id] = value;
    }

    public void AddHeader(string id, StringValues values)
    {
        if (string.IsNullOrEmpty(id)) return;

        foreach (var value in values.ToArray())
            Headers[id] = value;
    }

    public string? GetStatusCode()
    {
        if (string.IsNullOrEmpty(StatusCode)) return null;

        // Valid format of status code.
        if (StatusCodeRegex().IsMatch(StatusCode))
            return StatusCode;

        // Reformat status code to valid format.
        var statusCodeData = StatusCode.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return $"{statusCodeData[0]} ({string.Concat(statusCodeData.Skip(1))})";
    }

    public bool IsCorrupted()
    {
        return string.IsNullOrEmpty(StatusCode) || StartAt == DateTime.MinValue || string.IsNullOrEmpty(Method) || string.IsNullOrEmpty(TemplatePath);
    }

    public int Duration()
        => (int)Math.Round((EndAt - StartAt).TotalMilliseconds);

    [GeneratedRegex("\\d+\\s+\\([^\\)]+\\)")]
    private static partial Regex StatusCodeRegex();
}
