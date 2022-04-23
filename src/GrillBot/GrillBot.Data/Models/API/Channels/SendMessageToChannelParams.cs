using Discord;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Data.Models.API.Channels;

public class SendMessageToChannelParams
{
    [Required(ErrorMessage = "Obsah zprávy je povinný.")]
    [MinLength(1, ErrorMessage = "Minimální délka zprávy je 1 znak.")]
    [StringLength(DiscordConfig.MaxMessageSize, ErrorMessage = "Maximální délka zprávy je 2000 znaků.")]
    public string Content { get; set; }

    /// <summary>
    /// Reference is jump link or message ID.
    /// </summary>
    public string Reference { get; set; }
}
