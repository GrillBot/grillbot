using GrillBot.Data.Extensions.Discord;

namespace GrillBot.App.Extensions.Discord
{
    static public class GuildExtensions
    {
        static public IRole GetHighestRole(this IGuild guild, bool requireColor = false)
        {
            var roles = requireColor ? guild.Roles.Where(o => o.Color != Color.Default) : guild.Roles.AsEnumerable();

            return roles.OrderByDescending(o => o.Position).FirstOrDefault();
        }

        static public IEnumerable<string> GetTranslatedFeatures(this IGuild guild)
        {
            if (guild.Features.Value == GuildFeature.None)
                yield break;

            var features = Enum.GetValues<GuildFeature>()
                .Where(o => o > 0 && guild.Features.HasFeature(o));

            foreach (var feature in features)
            {
                switch (feature)
                {
                    case GuildFeature.AnimatedIcon:
                        yield return "Animovaná ikona";
                        break;
                    case GuildFeature.Banner:
                        yield return "Banner";
                        break;
                    case GuildFeature.Commerce:
                        yield return "eKomerce";
                        break;
                    case GuildFeature.Community:
                        yield return "Komunitní režim";
                        break;
                    case GuildFeature.Discoverable:
                        yield return "Veřejně viditelný";
                        break;
                    case GuildFeature.InviteSplash:
                        yield return "Pozadí u pozvánky";
                        break;
                    case GuildFeature.MemberVerificationGateEnabled:
                        yield return "Verifikace při připojení";
                        break;
                    case GuildFeature.News:
                        yield return "Novinky";
                        break;
                    case GuildFeature.Partnered:
                        yield return "Partnerský program";
                        break;
                    case GuildFeature.PreviewEnabled:
                        yield return "Náhled serveru před připojením.";
                        break;
                    case GuildFeature.VanityUrl:
                        yield return "Vanity URL";
                        break;
                    case GuildFeature.Verified:
                        yield return "Ověřený server";
                        break;
                    case GuildFeature.VIPRegions:
                        yield return "VIP hlasová oblast";
                        break;
                    case GuildFeature.WelcomeScreenEnabled:
                        yield return "Uvítací obrazovka";
                        break;
                    case GuildFeature.MonetizationEnabled:
                        yield return "Monetizace";
                        break;
                    case GuildFeature.MoreStickers:
                        yield return "Nálepky";
                        break;
                    case GuildFeature.PrivateThreads:
                        yield return "Privátní vlákna";
                        break;
                    case GuildFeature.RoleIcons:
                        yield return "Ikony rolí";
                        break;
                    case GuildFeature.SevenDayThreadArchive:
                        yield return "Archivace vláken po týdnu";
                        break;
                    case GuildFeature.ThreeDayThreadArchive:
                        yield return "Archivace vláken po 3 dnech";
                        break;
                    case GuildFeature.TicketedEventsEnabled:
                        yield return "Události";
                        break;
                }
            }

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
