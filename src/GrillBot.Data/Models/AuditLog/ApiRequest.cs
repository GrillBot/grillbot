using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GrillBot.Core.Infrastructure;
using Microsoft.Extensions.Primitives;

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
    public string? LoggedUserRole { get; set; }
    public string StatusCode { get; set; }
    public Dictionary<string, string> Parameters { get; set; } = new();
    public string Language { get; set; }
    public string? ApiGroupName { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public string UserIdentification { get; set; } = null!;
    public string IpAddress { get; set; }

    public void AddParameters(IDictionaryObject? apiObject, int index = -1)
    {
        if (apiObject == null) return;

        foreach (var item in apiObject.ToDictionary())
            AddParameter((index == -1 ? "" : index + ".") + item.Key, item.Value);
    }

    public void AddParameters(IEnumerable<IDictionaryObject> apiObjects)
    {
        var objects = apiObjects.ToArray();
        for (var i = 0; i < objects.Length; i++)
            AddParameters(objects[i], i);
    }

    public void AddParameter(string? id, string? value)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(value))
            return;
        Parameters[id] = value;
    }

    public void AddHeader(string id, StringValues values)
    {
        if (string.IsNullOrEmpty(id)) return;

        foreach (var value in values.Where(o => !string.IsNullOrEmpty(o)))
            Headers[id] = value!;
    }
}
