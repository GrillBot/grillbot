using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;

namespace GrillBot.App.Extensions.Discord
{
    static public class GuildExtensions
    {
        static public SocketRole GetHighestRole(this SocketGuild guild, bool requireColor = false)
        {
            var roles = requireColor ? guild.Roles.Where(o => o.Color != Color.Default) : guild.Roles.AsEnumerable();

            return roles.OrderByDescending(o => o.Position).FirstOrDefault();
        }

        static public IEnumerable<string> GetTranslatedFeatures(this SocketGuild guild)
        {
            if (guild.Features.Count == 0)
                yield break;

            foreach (var feature in guild.Features)
            {
                switch (feature.ToUpper())
                {
                    case "ANIMATED_ICON":
                        yield return "Animovaná ikona";
                        break;
                    case "BANNER":
                        yield return "Banner";
                        break;
                    case "COMMERCE":
                        yield return "eKomerce";
                        break;
                    case "COMMUNITY":
                        yield return "Komunitní režim";
                        break;
                    case "DISCOVERABLE":
                        yield return "Veřejně viditelný";
                        break;
                    case "INVITE_SPLASH":
                        yield return "Pozadí u pozvánky";
                        break;
                    case "MEMBER_VERIFICATION_GATE_ENABLED":
                        yield return "Verifikace při připojení";
                        break;
                    case "NEWS":
                        yield return "Novinky";
                        break;
                    case "PARTNERED":
                        yield return "Partnerský program";
                        break;
                    case "PREVIEW_ENABLED":
                        yield return "Náhled serveru před připojením.";
                        break;
                    case "VANITY_URL":
                        yield return "Vanity URL";
                        break;
                    case "VERIFIED":
                        yield return "Ověřený server";
                        break;
                    case "VIP_REGIONS":
                        yield return "VIP hlasová oblast";
                        break;
                    case "WELCOME_SCREEN_ENABLED":
                        yield return "Uvítací obrazovka";
                        break;
                }
            }
        }

        static public int CalculateFileUploadLimit(this SocketGuild guild)
        {
            return guild.PremiumTier switch
            {
                PremiumTier.Tier2 => 50,
                PremiumTier.Tier3 => 100,
                _ => 8
            };
        }

        static public IEnumerable<SocketTextChannel> GetAvailableChannelsFor(this SocketGuild guild, SocketGuildUser user)
        {
            return guild.TextChannels.Where(o => o.HaveAccess(user));
        }
    }
}
