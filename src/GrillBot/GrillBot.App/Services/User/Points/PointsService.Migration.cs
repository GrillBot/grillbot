using GrillBot.Database.Entity;

namespace GrillBot.App.Services.User.Points;

public partial class PointsService
{
    private PointsTransaction CreateMigratedTransaction(GuildUser guildUser, PointsTransaction referenceTransaction)
    {
        if (guildUser.Points <= 0) return null;

        var transaction = CreateTransaction(guildUser, false, 0, true);
        transaction.Points = referenceTransaction.Points * 2;

        guildUser.Points -= transaction.Points;
        if (guildUser.Points < 0) guildUser.Points = 0;

        return transaction;
    }
}
