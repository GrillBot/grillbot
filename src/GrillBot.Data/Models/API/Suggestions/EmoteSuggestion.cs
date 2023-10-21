using System;
using AutoMapper;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Data.Models.API.Users;

namespace GrillBot.Data.Models.API.Suggestions;

public class EmoteSuggestion
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? VoteEndsAt { get; set; }
    public byte[] ImageData { get; set; } = null!;
    public Guild Guild { get; set; } = null!;
    public GuildUser FromUser { get; set; } = null!;
    public string EmoteName { get; set; } = null!;
    public string? Description { get; set; }
    public bool? ApprovedForVote { get; set; }
    public bool VoteFinished { get; set; }
    public bool CommunityApproved { get; set; }
    public int UpVotes { get; set; }
    public int DownVotes { get; set; }
}

public class EmoteSuggestionMappingProfile : Profile
{
    public EmoteSuggestionMappingProfile()
    {
        CreateMap<Database.Entity.EmoteSuggestion, EmoteSuggestion>();
    }
}
