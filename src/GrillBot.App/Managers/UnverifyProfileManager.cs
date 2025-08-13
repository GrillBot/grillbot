using GrillBot.Data.Models.Unverify;

namespace GrillBot.App.Managers;

public class UnverifyProfileManager
{
    public static UnverifyUserProfile Reconstruct(Database.Entity.Unverify unverify, IGuildUser toUser, IGuild guild)
    {
        var logData = JsonConvert.DeserializeObject<UnverifyLogSet>(unverify.UnverifyLog!.Data);

        return logData is null
            ? throw new ArgumentException("Missing log data for unverify reconstruction.")
            : new UnverifyUserProfile(toUser, unverify.StartAt, unverify.EndAt, logData.IsSelfUnverify, logData.Language ?? "cs")
            {
                ChannelsToKeep = logData.ChannelsToKeep,
                ChannelsToRemove = logData.ChannelsToRemove,
                Reason = logData.Reason,
                RolesToKeep = logData.RolesToKeep.Select(guild.GetRole).Where(o => o != null).ToList(),
                RolesToRemove = logData.RolesToRemove.Select(guild.GetRole).Where(o => o != null).ToList(),
                KeepMutedRole = logData.KeepMutedRole
            };
    }
}
