using Discord;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.Data.Extensions.Discord;

public static class UserExtensions
{
    public static string GetDisplayName(this IUser user, bool withDiscriminator = true)
    {
        return user switch
        {
            null => "Neznámý uživatel",
            IGuildUser sgu when !string.IsNullOrEmpty(sgu.Nickname) => sgu.Nickname,
            _ => withDiscriminator ? $"{user.Username}#{user.Discriminator}" : user.Username
        };
    }

    public static bool IsUser(this IUser user) => !(user.IsBot || user.IsWebhook);
    public static bool HaveAnimatedAvatar(this IUser user) => user.AvatarId?.StartsWith("a_") ?? false;
    public static string CreateProfilePicFilename(this IUser user, int size) => $"{user.Id}_{user.AvatarId ?? user.Discriminator}_{size}.{(user.HaveAnimatedAvatar() ? "gif" : "png")}";

    public static IRole GetHighestRole(this SocketGuildUser user, bool requireColor = false)
    {
        var roles = requireColor ? user.Roles.Where(o => o.Color != Color.Default) : user.Roles.AsEnumerable();

        return roles.MaxBy(o => o.Position);
    }

    public static Task TryAddRoleAsync(this IGuildUser user, IRole role)
    {
        return user.RoleIds.Any(o => o == role.Id) ? Task.CompletedTask : user.AddRoleAsync(role);
    }

    public static Task TryRemoveRoleAsync(this IGuildUser user, IRole role)
    {
        return user.RoleIds.All(o => o != role.Id) ? Task.CompletedTask : user.RemoveRoleAsync(role);
    }

    public static int CalculateJoinPosition(this SocketGuildUser user)
    {
        return user.Guild.Users.Count(o => o.JoinedAt <= user.JoinedAt);
    }
}
