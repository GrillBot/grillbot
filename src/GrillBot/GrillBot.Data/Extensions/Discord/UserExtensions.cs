using Discord;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.Data.Extensions.Discord;

static public class UserExtensions
{
    static public string GetDisplayName(this IUser user, bool withDiscriminator = true)
    {
        if (user is IGuildUser sgu && !string.IsNullOrEmpty(sgu.Nickname))
            return sgu.Nickname;

        return withDiscriminator ? $"{user.Username}#{user.Discriminator}" : user.Username;
    }

    static public bool IsUser(this IUser user) => !(user.IsBot || user.IsWebhook);

    static public string GetFullName(this IUser user)
    {
        if (user is IGuildUser sgu && !string.IsNullOrEmpty(sgu.Nickname))
            return $"{sgu.Nickname} ({sgu.Username}#{sgu.Discriminator})";

        return $"{user.Username}#{user.Discriminator}";
    }

    static public bool HaveAnimatedAvatar(this IUser user) => user.AvatarId?.StartsWith("a_") ?? false;
    static public string CreateProfilePicFilename(this IUser user, int size) => $"{user.Id}_{user.AvatarId ?? user.Discriminator}_{size}.{(user.HaveAnimatedAvatar() ? "gif" : "png")}";

    static public IRole GetHighestRole(this SocketGuildUser user, bool requireColor = false)
    {
        var roles = requireColor ? user.Roles.Where(o => o.Color != Color.Default) : user.Roles.AsEnumerable();

        return roles.OrderByDescending(o => o.Position).FirstOrDefault();
    }

    public static Task TryAddRoleAsync(this IGuildUser user, IRole role)
    {
        return user.RoleIds.Any(o => o == role.Id) ? Task.CompletedTask : user.AddRoleAsync(role);
    }

    public static Task TryRemoveRoleAsync(this IGuildUser user, IRole role)
    {
        return !user.RoleIds.Any(o => o == role.Id) ? Task.CompletedTask : user.RemoveRoleAsync(role);
    }

    public static int CalculateJoinPosition(this SocketGuildUser user)
    {
        return user.Guild.Users.Count(o => o.JoinedAt <= user.JoinedAt);
    }
}
