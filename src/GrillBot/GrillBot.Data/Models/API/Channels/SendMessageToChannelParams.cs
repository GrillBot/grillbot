using System.Collections.Generic;
using Discord;
using System.ComponentModel.DataAnnotations;
using GrillBot.Common.Infrastructure;
using Newtonsoft.Json;
using NSwag.Annotations;

namespace GrillBot.Data.Models.API.Channels;

public class SendMessageToChannelParams : IApiObject
{
    [Required(ErrorMessage = "Obsah zprávy je povinný.")]
    [MinLength(1, ErrorMessage = "Minimální délka zprávy je 1 znak.")]
    [StringLength(DiscordConfig.MaxMessageSize, ErrorMessage = "Maximální délka zprávy je 2000 znaků.")]
    public string Content { get; set; }

    /// <summary>
    /// Reference is jump link or message ID.
    /// </summary>
    public string Reference { get; set; }

    // Only for commands.
    [JsonIgnore]
    [OpenApiIgnore]
    public List<FileAttachment> Attachments { get; } = new();

    public Dictionary<string, string> SerializeForLog()
    {
        return new Dictionary<string, string>
        {
            { "ContentLength", Content.Length.ToString() },
            { nameof(Reference), Reference }
        };
    }
}
