using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Data.Models.API.Users;

namespace GrillBot.Data.Models.API.Searching;

public class SearchingListItem
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Who searching
    /// </summary>
    public User User { get; set; }

    /// <summary>
    /// Server where was created search.
    /// </summary>
    public Guild Guild { get; set; }

    /// <summary>
    /// Channel where was created search.
    /// </summary>
    public Channel Channel { get; set; }

    /// <summary>
    /// Message content
    /// </summary>
    public string Message { get; set; }
}

public class SearchingMappingProfile : AutoMapper.Profile
{
    public SearchingMappingProfile()
    {
        CreateMap<Database.Entity.SearchItem, SearchingListItem>()
            .ForMember(dst => dst.Message, opt => opt.MapFrom(src => src.MessageContent));
    }
}
