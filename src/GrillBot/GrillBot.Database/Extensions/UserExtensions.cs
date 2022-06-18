using GrillBot.Database.Entity;

namespace GrillBot.Database.Extensions;

public static class UserExtensions
{
    public static string FullName(this GuildUser user, bool noDiscriminator = false)
    {
        var username = $"{user.User!.Username}{(noDiscriminator ? "" : $"#{user.User!.Discriminator}")}";
        return !string.IsNullOrEmpty(user.Nickname) ? $"{user.Nickname} ({username})" : username;
    }
}
