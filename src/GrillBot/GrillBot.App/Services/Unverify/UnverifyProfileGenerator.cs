using GrillBot.Common.Extensions.Discord;
using GrillBot.Data.Models;
using GrillBot.Data.Models.Unverify;

namespace GrillBot.App.Services.Unverify;

public class UnverifyProfileGenerator
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public UnverifyProfileGenerator(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task<UnverifyUserProfile> CreateAsync(IGuildUser user, IGuild guild, DateTime end, string data, bool selfunverify, List<string> keep, IRole mutedRole)
    {
        var profile = new UnverifyUserProfile(user, DateTime.Now, end, selfunverify)
        {
            Reason = !selfunverify ? ParseReason(data) : null
        };

        var keepables = await GetKeepablesAsync();

        await ProcessRolesAsync(profile, user, guild, selfunverify, keep, mutedRole, keepables);
        await ProcessChannelsAsync(profile, guild, user, keep, keepables);

        return profile;
    }

    public static UnverifyUserProfile Reconstruct(Database.Entity.Unverify unverify, IGuildUser toUser, IGuild guild)
    {
        var logData = JsonConvert.DeserializeObject<UnverifyLogSet>(unverify.UnverifyLog!.Data);
        if (logData == null)
            throw new ArgumentException("Missing log data for unverify reconstruction.");

        return new UnverifyUserProfile(toUser, unverify.StartAt, unverify.EndAt, logData.IsSelfUnverify)
        {
            ChannelsToKeep = logData.ChannelsToKeep,
            ChannelsToRemove = logData.ChannelsToRemove,
            Reason = logData.Reason,
            RolesToKeep = logData.RolesToKeep.Select(guild.GetRole).Where(o => o != null).ToList(),
            RolesToRemove = logData.RolesToRemove.Select(guild.GetRole).Where(o => o != null).ToList()
        };
    }

    private static string ParseReason(string data)
    {
        var ex = new ValidationException("Nelze bezdůvodně odebrat přístup. Přečti si nápovědu a pak to zkus znovu.");

        var parts = data.Split("<@", StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) throw ex;

        var reason = parts[0].Trim();
        if (string.IsNullOrEmpty(reason)) throw ex;
        return reason;
    }

    private static async Task ProcessRolesAsync(UnverifyUserProfile profile, IGuildUser user, IGuild guild, bool selfunverify, List<string> keep, IRole mutedRole,
        Dictionary<string, List<string>> keepables)
    {
        var rolesToRemove = user.GetRoles();
        profile.RolesToRemove.AddRange(rolesToRemove);

        if (selfunverify)
        {
            var currentUser = await guild.GetCurrentUserAsync();
            var botRolePosition = currentUser.GetRoles().Max(o => o.Position);
            var rolesToKeep = profile.RolesToRemove.FindAll(o => o.Position >= botRolePosition);

            if (rolesToKeep.Count > 0)
            {
                profile.RolesToKeep.AddRange(rolesToKeep);
                profile.RolesToRemove.RemoveAll(o => rolesToKeep.Any(x => x.Id == o.Id));
            }
        }

        var unavailable = profile.RolesToRemove.FindAll(o => o.IsManaged || (mutedRole != null && o.Id == mutedRole.Id));
        if (unavailable.Count > 0)
        {
            profile.RolesToKeep.AddRange(unavailable);
            profile.RolesToRemove.RemoveAll(o => unavailable.Any(x => x.Id == o.Id));
        }

        foreach (var toKeep in keep)
        {
            CheckDefinition(keepables, toKeep);
            var role = profile.RolesToRemove.Find(o => string.Equals(o.Name, toKeep, StringComparison.InvariantCultureIgnoreCase));

            if (role != null)
            {
                profile.RolesToKeep.Add(role);
                profile.RolesToRemove.Remove(role);
                continue;
            }

            foreach (var groupKey in keepables.Where(o => o.Value?.Contains(toKeep) == true).Select(o => o.Key))
            {
                role = profile.RolesToRemove.Find(o => string.Equals(o.Name, groupKey == "_" ? toKeep : groupKey, StringComparison.InvariantCultureIgnoreCase));
                if (role == null)
                    continue;

                profile.RolesToKeep.Add(role);
                profile.RolesToRemove.Remove(role);
            }
        }
    }

    private static async Task ProcessChannelsAsync(UnverifyUserProfile profile, IGuild guild, IUser user, List<string> keep, Dictionary<string, List<string>> keepabless)
    {
        var channels = (await guild.GetChannelsAsync()).ToList();
        channels = channels
            .FindAll(o => o is (IVoiceChannel or ITextChannel) and not IThreadChannel); // Select channels but ignore channels

        var channelsToRemove = channels
            .Select(o => new ChannelOverride(o, o.GetPermissionOverwrite(user) ?? OverwritePermissions.InheritAll))
            .Where(o => o.AllowValue > 0 || o.DenyValue > 0);

        profile.ChannelsToRemove.AddRange(channelsToRemove);
        foreach (var toKeep in keep)
        {
            CheckDefinition(keepabless, toKeep);
            foreach (var overwrite in profile.ChannelsToRemove.ToList())
            {
                var guildChannel = await guild.GetChannelAsync(overwrite.ChannelId);

                if (guildChannel == null) continue;
                if (!string.Equals(guildChannel.Name, toKeep)) continue;

                profile.ChannelsToKeep.Add(overwrite);
                profile.ChannelsToRemove.RemoveAll(o => o.ChannelId == overwrite.ChannelId);
            }
        }
    }

    private static bool ExistsInKeepDefinition(Dictionary<string, List<string>> definitions, string item)
    {
        return definitions.ContainsKey(item) || definitions.Values.Any(o => o?.Contains(item) == true);
    }

    private static void CheckDefinition(Dictionary<string, List<string>> definitions, string item)
    {
        if (!ExistsInKeepDefinition(definitions, item))
            throw new ValidationException($"{item.ToUpper()} není ponechatelné.");
    }

    private async Task<Dictionary<string, List<string>>> GetKeepablesAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var keepables = await repository.SelfUnverify.GetKeepablesAsync();
        return keepables.GroupBy(o => o.GroupName.ToUpper())
            .ToDictionary(o => o.Key, o => o.Select(x => x.Name.ToUpper()).ToList());
    }
}
