using System.Collections.Generic;
using GrillBot.Common.Infrastructure;
using GrillBot.Data.Infrastructure.Validation;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using Newtonsoft.Json;

namespace GrillBot.Data.Models.API.AuditLog.Filters;

public class MessageDeletedFilter : IExtendedFilter, IApiObject
{
    public bool? ContainsEmbed { get; set; }
    public string ContentContains { get; set; }

    [DiscordId]
    public string AuthorId { get; set; }

    private bool IsAuthorIdSet => !string.IsNullOrEmpty(AuthorId) && ulong.TryParse(AuthorId, out var authorId) && authorId > 0;

    public bool IsSet()
    {
        return ContainsEmbed != null || !string.IsNullOrEmpty(ContentContains) || IsAuthorIdSet;
    }

    public bool IsValid(AuditLogItem item, JsonSerializerSettings settings)
    {
        var data = JsonConvert.DeserializeObject<MessageDeletedData>(item.Data, settings)!.Data;

        if (ContainsEmbed != null)
        {
            switch (ContainsEmbed)
            {
                case true when data.Embeds == null || data.Embeds.Count == 0:
                case false when data.Embeds is { Count: > 0 }:
                    return false;
            }
        }

        if (!string.IsNullOrEmpty(ContentContains) && (string.IsNullOrEmpty(data.Content) || !data.Content.Contains(ContentContains)))
            return false;
        return !IsAuthorIdSet || data.Author.Id == ulong.Parse(AuthorId);
    }

    public Dictionary<string, string> SerializeForLog()
    {
        return new Dictionary<string, string>
        {
            { nameof(ContainsEmbed), ContainsEmbed?.ToString() },
            { nameof(ContentContains), ContentContains },
            { nameof(AuthorId), AuthorId }
        };
    }
}
