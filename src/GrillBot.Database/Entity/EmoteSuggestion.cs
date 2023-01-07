using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Discord;

namespace GrillBot.Database.Entity;

public class EmoteSuggestion
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    [StringLength(30)]
    public string SuggestionMessageId { get; set; } = null!;
    
    [StringLength(30)]
    public string? VoteMessageId { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    public DateTime? VoteEndsAt { get; set; }

    [Required]
    public byte[] ImageData { get; set; } = null!;

    [Required]
    [StringLength(30)]
    public string GuildId { get; set; } = null!;

    [ForeignKey(nameof(GuildId))]
    public Guild Guild { get; set; } = null!;

    [Required]
    [StringLength(30)]
    public string FromUserId { get; set; } = null!;

    public GuildUser FromUser { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Filename { get; set; } = null!;

    [StringLength(50)]
    [MinLength(2)]
    [Required]
    public string EmoteName { get; set; } = null!;
    
    [StringLength(EmbedFieldBuilder.MaxFieldValueLength)]
    public string? Description { get; set; }
    
    public bool? ApprovedForVote { get; set; }
    
    public bool VoteFinished { get; set; }
    
    public bool CommunityApproved { get; set; }
    
    public int UpVotes { get; set; }
    
    public int DownVotes { get; set; }
}
