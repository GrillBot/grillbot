namespace GrillBot.Data.Models.API.Points;

public class PointsMappingProfile : AutoMapper.Profile
{
    public PointsMappingProfile()
    {
        CreateMap<Database.Entity.PointsTransaction, PointsTransaction>()
            .ForMember(dst => dst.User, opt => opt.MapFrom(src => src.GuildUser.User))
            .ForMember(dst => dst.AssignedAt, opt => opt.MapFrom(src => src.AssingnedAt));

        CreateMap<Database.Entity.PointsTransactionSummary, PointsSummaryBase>()
            .ForMember(dst => dst.TotalPoints, opt => opt.MapFrom(src => src.MessagePoints + src.ReactionPoints));

        CreateMap<Database.Entity.PointsTransactionSummary, PointsSummary>()
            .IncludeBase<Database.Entity.PointsTransactionSummary, PointsSummaryBase>()
            .ForMember(dst => dst.User, opt => opt.MapFrom(src => src.GuildUser.User));
    }
}
