using Discord;
using Discord.WebSocket;
using System.Linq;

namespace GrillBot.Data.Extensions.Discord
{
    static public class ChannelExtensions
    {
        static public string GetMention(this IChannel channel) => $"<#{channel.Id}>";

        static public bool HaveAccess(this SocketGuildChannel channel, SocketGuildUser user)
        {
            if (channel.GetUser(user.Id) != null || channel.PermissionOverwrites.Count == 0)
                return true;

            var overwrite = channel.GetPermissionOverwrite(user);
            if (overwrite != null)
            {
                if (overwrite.Value.ViewChannel == PermValue.Allow)
                    return true;
                else if (overwrite.Value.ViewChannel == PermValue.Deny)
                    return false;
            }

            var everyonePerm = channel.GetPermissionOverwrite(user.Guild.EveryoneRole);
            var isEveryonePerm = everyonePerm != null && (everyonePerm.Value.ViewChannel == PermValue.Allow || everyonePerm.Value.ViewChannel == PermValue.Inherit);

            foreach (var role in user.Roles.Where(o => !o.IsEveryone))
            {
                var roleOverwrite = channel.GetPermissionOverwrite(role);
                if (roleOverwrite == null) continue;

                if (roleOverwrite.Value.ViewChannel == PermValue.Deny && isEveryonePerm)
                    return false;

                if (roleOverwrite.Value.ViewChannel == PermValue.Allow)
                    return true;
            }

            return isEveryonePerm;
        }

        public static bool IsEqual(this IGuildChannel channel, IGuildChannel another)
        {
            if (channel.GetType() != another.GetType()) return false;
            if (channel.Id != another.Id) return false;
            if (channel.Name != another.Name) return false;
            if (channel.Position != another.Position) return false;

            if (channel is ITextChannel textChannel && another is ITextChannel anotherTextChannel)
            {
                if (textChannel.CategoryId != anotherTextChannel.CategoryId) return false;
                if (textChannel.IsNsfw != anotherTextChannel.IsNsfw) return false;
                if (textChannel.SlowModeInterval != anotherTextChannel.SlowModeInterval) return false;
                if (textChannel.Topic != anotherTextChannel.Topic) return false;
            }

            if (channel is IVoiceChannel voiceChannel && another is IVoiceChannel anotherVoiceChannel)
            {
                if (voiceChannel.Bitrate != anotherVoiceChannel.Bitrate) return false;
                if (voiceChannel.CategoryId != anotherVoiceChannel.CategoryId) return false;
                if (voiceChannel.UserLimit != anotherVoiceChannel.UserLimit) return false;
            }

            return true;
        }

        public static ICategoryChannel GetCategory(this SocketGuildChannel channel)
        {
            if (channel is SocketCategoryChannel categoryChannel) return categoryChannel;
            else if (channel is SocketTextChannel textChannel) return textChannel.Category;
            else if (channel is SocketVoiceChannel voiceChannel) return voiceChannel.Category;

            return null;
        }
    }
}
