namespace GrillBot.Data.Models.API.AutoReply;

public class AutoReplyItem
{
    public long Id { get; set; }
    public string Template { get; set; } = null!;
    public string Reply { get; set; } = null!;
    public long Flags { get; set; }
}

public class AutoReplyItemMappingProfile : AutoMapper.Profile
{
    public AutoReplyItemMappingProfile()
    {
        CreateMap<Database.Entity.AutoReplyItem, AutoReplyItem>();
    }
}
