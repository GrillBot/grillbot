using System;
using System.Collections.Generic;
using System.Linq;
using Discord;

namespace GrillBot.Data.Models.AuditLog;

public class EmbedInfo
{
    public string Title { get; set; }
    public string Type { get; set; }
    public string ImageInfo { get; set; }
    public string VideoInfo { get; set; }
    public string AuthorName { get; set; }
    public bool ContainsFooter { get; set; }
    public string ProviderName { get; set; }
    public string ThumbnailInfo { get; set; }
    public int FieldsCount { get; set; }
    public List<EmbedFieldInfo> Fields { get; set; }

    public EmbedInfo()
    {
    }

    public EmbedInfo(IEmbed embed)
    {
        Title = embed.Title;
        Type = embed.Type.ToString();
        AuthorName = embed.Author?.Name;
        ContainsFooter = embed.Footer != null;
        ProviderName = embed.Provider?.Name;
        FieldsCount = embed.Fields.Length;
        VideoInfo = ParseVideoInfo(embed.Video);

        if (embed.Image != null)
            ImageInfo = $"{embed.Image!.Value.Url} ({embed.Image.Value.Width}x{embed.Image.Value.Height})";

        if (embed.Thumbnail != null)
            ThumbnailInfo = $"{embed.Thumbnail!.Value.Url} ({embed.Thumbnail.Value.Width}x{embed.Thumbnail.Value.Height})";

        if (FieldsCount > 0)
            Fields = embed.Fields.Select(o => new EmbedFieldInfo(o)).ToList();
    }

    private string ParseVideoInfo(EmbedVideo? video)
    {
        if (video == null)
            return null;

        var size = $"({video!.Value.Width}x{video.Value.Height})";

        if (ProviderName != "Twitch")
            return $"{video.Value.Url} {size}";

        var url = new Uri(video.Value.Url);
        var queryFields = url.Query[1..].Split('&').Select(o => o.Split('=')).ToDictionary(o => o[0], o => o[1]);
        return $"{queryFields["channel"]} {size}";
    }
}
