using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.Data.Extensions.Discord;

public static class GuildExtensions
{
    private static Dictionary<string, string> FeaturesList { get; } = new()
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

    public static IRole GetHighestRole(this IGuild guild, bool requireColor = false)
    {
        var roles = requireColor ? guild.Roles.Where(o => o.Color != Color.Default) : guild.Roles.AsEnumerable();

        return roles.MaxBy(o => o.Position);
    }

    public static IEnumerable<string> GetTranslatedFeatures(this IGuild guild)
    {
        if (guild.Features.Value == GuildFeature.None)
            return Enumerable.Empty<string>();

        return Enum.GetValues<GuildFeature>()
            .Where(o => o > 0 && guild.Features.HasFeature(o))
            .Select(o => o.ToString())
            .Select(o => FeaturesList.TryGetValue(o, out var text) ? text : o)
            .Where(o => !string.IsNullOrEmpty(o))
            .Distinct()
            .OrderBy(o => o);
    }

    public static int CalculateFileUploadLimit(this IGuild guild)
        => Convert.ToInt32((guild?.MaxUploadLimit ?? 0) / 1000000);

    private static IEnumerable<SocketTextChannel> GetAvailableTextChannelsFor(this SocketGuild guild, SocketGuildUser user, bool includeThreads = false)
    {
        var query = guild.TextChannels.AsEnumerable();

        if (!includeThreads)
            query = query.Where(o => o is not SocketThreadChannel);

        return query.Where(o => o.HaveAccess(user));
    }

    public static IEnumerable<SocketGuildChannel> GetAvailableChannelsFor(this SocketGuild guild, SocketGuildUser user, bool includeThreads = false)
    {
        return GetAvailableTextChannelsFor(guild, user, includeThreads)
            .Select(o => o as SocketGuildChannel)
            .Where(o => o != null)
            .Concat(guild.VoiceChannels.Where(o => o.HaveAccess(user)));
    }

    public static async Task<List<IGuildChannel>> GetAvailableChannelsAsync(this IGuild guild, IGuildUser user, bool onlyText = false)
    {
        var allChannels = (onlyText ? (await guild.GetTextChannelsAsync()).OfType<IGuildChannel>().ToList() : (await guild.GetChannelsAsync()).ToList())
            .Where(o => o is not IThreadChannel);

        return await allChannels.FindAllAsync(o => o.HaveAccessAsync(user));
    }
}
