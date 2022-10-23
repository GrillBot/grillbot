using GrillBot.Data.Models.API.Users;
using System;

namespace GrillBot.Data.Models.API.Reminder;

public class RemindMessage
{
    public long Id { get; set; }
    public User FromUser { get; set; }
    public User ToUser { get; set; }
    public DateTime At { get; set; }
    public string Message { get; set; }
    public int Postpone { get; set; }
    public bool Notified { get; set; }
    public string Language { get; set; }
}

public class RemindMappingProfile : AutoMapper.Profile
{
    public RemindMappingProfile()
    {
        CreateMap<Database.Entity.RemindMessage, RemindMessage>()
            .ForMember(dst => dst.Notified, opt => opt.MapFrom(src => src.RemindMessageId != null));
    }
}
