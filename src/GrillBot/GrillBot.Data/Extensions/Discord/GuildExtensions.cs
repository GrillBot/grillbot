using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GrillBot.Data.Extensions.Discord
{
    static public class GuildExtensions
    {
        private static Dictionary<string, string> FeaturesList { get; } = new Dictionary<string, string>()
        {
            { "AnimatedIcon", "Animovaná ikona" },
            { "Banner", "Banner" },
            { "Commerce", "eKomerce" },
            { "Community", "Komunitní režim" },
            { "Discoverable", "Veřejně viditelný" },
            { "InviteSplash", "Pozadí u pozvánky" },
            { "MemberVerificationGateEnabled", "Verifikace při připojení" },
            { "News", "Novinky" },
            { "Partnered", "Partnerský program" },
            { "PreviewEnabled", "Náhled serveru před připojením" },
            { "VanityUrl", "Vanity URL" },
            { "Verified", "Ověřený server" },
            { "VIPRegions", "VIP hlasová oblast" },
            { "WelcomeScreenEnabled", "Uvítací obrazovka" },
            { "MonetizationEnabled", "Monetizace" },
            { "MoreStickers", "Nálepky" },
            { "PrivateThreads", "Privátní vlákna" },
            { "RoleIcons", "Ikony rolí" },
            { "SevenDayThreadArchive", "Archivace vláken po týdnu" },
            { "ThreeDayThreadArchive", "Archivace vláken po 3 dnech" },
            { "TicketedEventsEnabled", "Události" },
            { "AnimatedBanner", "Animovaný banner" },
            { "TextInVoiceEnabled", "Text v hlasových kanálech" },
            { "ThreadsEnabled", "Vlákna" },
            { "ChannelBanner", "Banner kanálů" },
            { "Hub", "Školní server" },
            { "MoreEmoji", "Více emotů" },
            { "RoleSubscriptionsAvailableForPurchase", "Placené role" },
            { "MemberProfiles", "Alternativní profily uživatelů" },
            { "NewThreadPermissions", "" },
            { "ThreadsEnabledTesting", "" }
        };

        static public IRole GetHighestRole(this IGuild guild, bool requireColor = false)
        {
            var roles = requireColor ? guild.Roles.Where(o => o.Color != Color.Default) : guild.Roles.AsEnumerable();

            return roles.OrderByDescending(o => o.Position).FirstOrDefault();
        }

        static public IEnumerable<string> GetTranslatedFeatures(this IGuild guild)
        {
            if (guild.Features.Value == GuildFeature.None)
                return Enumerable.Empty<string>();

            return Enum.GetValues<GuildFeature>()
                .Where(o => o > 0 && guild.Features.HasFeature(o))
                .Select(o => o.ToString())
                .Select(o => FeaturesList.TryGetValue(o, out string text) ? text : o)
                .Where(o => !string.IsNullOrEmpty(o))
                .Distinct()
                .OrderBy(o => o);
        }

        static public int CalculateFileUploadLimit(this IGuild guild)
        {
            return guild?.PremiumTier switch
            {
                PremiumTier.Tier2 => 50,
                PremiumTier.Tier3 => 100,
                _ => 8
            };
        }

        static public IEnumerable<SocketTextChannel> GetAvailableTextChannelsFor(this SocketGuild guild, SocketGuildUser user)
        {
            return guild.TextChannels
                .Where(o => o is not SocketThreadChannel && o.HaveAccess(user));
        }

        static public IEnumerable<SocketGuildChannel> GetAvailableChannelsFor(this SocketGuild guild, SocketGuildUser user)
        {
            return GetAvailableTextChannelsFor(guild, user)
                .Select(o => o as SocketGuildChannel)
                .Where(o => o != null)
                .Concat(guild.VoiceChannels.Where(o => o.HaveAccess(user)));
        }
    }
}
