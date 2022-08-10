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

        CreateMap<Database.Entity.PointsTransactionSummary, PointsMergeInfo>()
            .ForMember(dst => dst.MergeRangeFrom, opt => opt.MapFrom(src => src.MergeRangeFrom.GetValueOrDefault()))
            .ForMember(dst => dst.MergeRangeTo, opt =>
            {
                opt.PreCondition(src => src.MergeRangeFrom.GetValueOrDefault() != src.MergeRangeTo.GetValueOrDefault());
                opt.MapFrom(src => src.MergeRangeTo.GetValueOrDefault());
            });

        CreateMap<Database.Entity.PointsTransaction, PointsTransaction>()
            .ForMember(dst => dst.MergeInfo, opt => opt.Ignore())
            .ForMember(dst => dst.User, opt => opt.MapFrom(src => src.GuildUser.User))
            .ForMember(dst => dst.AssignedAt, opt => opt.MapFrom(src => src.AssingnedAt))
            .ForMember(dst => dst.IsReaction, opt => opt.MapFrom(src => src.IsReaction()));

        CreateMap<Database.Entity.PointsTransactionSummary, PointsSummaryBase>()
            .ForMember(dst => dst.TotalPoints, opt => opt.MapFrom(src => src.MessagePoints + src.ReactionPoints));

        CreateMap<Database.Entity.PointsTransactionSummary, PointsSummary>()
            .IncludeBase<Database.Entity.PointsTransactionSummary, PointsSummaryBase>()
            .ForMember(dst => dst.MergeInfo, opt => opt.Ignore())
            .ForMember(dst => dst.User, opt => opt.MapFrom(src => src.GuildUser.User));
    }
}
