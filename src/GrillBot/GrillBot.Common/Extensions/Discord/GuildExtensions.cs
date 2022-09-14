using Discord;

namespace GrillBot.Common.Extensions.Discord;

public static class GuildExtensions
{
    public static IRole? GetHighestRole(this IGuild guild, bool requireColor = false)
    {
        var roles = requireColor ? guild.Roles.Where(o => o.Color != Color.Default) : guild.Roles.AsEnumerable();
        return roles.MaxBy(o => o.Position);
    }

    public static int CalculateFileUploadLimit(this IGuild guild)
        => Convert.ToInt32(guild.MaxUploadLimit / 1000000);

    public static async Task<List<IGuildChannel>> GetAvailableChannelsAsync(this IGuild guild, IGuildUser user, bool onlyText = false)
    {
        var allChannels = (onlyText ? (await guild.GetTextChannelsAsync()).OfType<IGuildChannel>().ToList() : (await guild.GetChannelsAsync()).ToList())
            .Where(o => o is not IThreadChannel);

        return await allChannels.FindAllAsync(o => o.HaveAccessAsync(user));
    }

    public static async Task<bool> CanManageInvitesAsync(this IGuild guild, IUser user)
    {
        var guildUser = user as IGuildUser ?? await guild.GetUserAsync(user.Id);
        return guildUser != null && guildUser.CanManageInvites();
    }
}
