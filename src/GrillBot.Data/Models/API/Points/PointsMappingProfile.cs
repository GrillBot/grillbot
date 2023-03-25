namespace GrillBot.Data.Models.API.Points;

public class PointsMappingProfile : AutoMapper.Profile
{
    public PointsMappingProfile()
    {
        CreateMap<Database.Entity.PointsTransaction, PointsMergeInfo>()
            .ForMember(dst => dst.MergeRangeFrom, opt => opt.MapFrom(src => src.MergeRangeFrom.GetValueOrDefault()))
            .ForMember(dst => dst.MergeRangeTo, opt =>
            {
                opt.PreCondition(src => src.MergeRangeFrom.GetValueOrDefault() != src.MergeRangeTo.GetValueOrDefault());
                opt.MapFrom(src => src.MergeRangeTo.GetValueOrDefault());
            });

        CreateMap<Database.Entity.PointsTransaction, PointsTransaction>()
            .ForMember(dst => dst.MergeInfo, opt => opt.Ignore())
            .ForMember(dst => dst.User, opt => opt.MapFrom(src => src.GuildUser.User))
            .ForMember(dst => dst.CreatedAt, opt => opt.MapFrom(src => src.AssingnedAt))
            .ForMember(dst => dst.IsReaction, opt => opt.MapFrom(src => src.IsReaction()));
    }
}
