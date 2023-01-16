using Discord.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Managers;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Database.Enums;

namespace GrillBot.App.Modules.Interactions;

[Group("guild", "Guild management")]
[RequireUserPerms]
[RequireBotPermission(GuildPermission.Administrator)]
public class GuildModule : InteractionsModuleBase
{
    private UserManager UserManager { get; }
    private GuildHelper GuildHelper { get; }

    public GuildModule(GuildHelper guildHelper, IServiceProvider serviceProvider, UserManager userManager) : base(serviceProvider)
    {
        GuildHelper = guildHelper;
        UserManager = userManager;
    }

    [SlashCommand("info", "Guild information")]
    public async Task GetInfoAsync()
    {
        var guild = Context.Guild;

        var basicEmotesCount = guild.Emotes.Count(o => !o.Animated);
        var animatedCount = guild.Emotes.Count - basicEmotesCount;
        var banCount = (await guild.GetBansAsync().FlattenAsync()).Count();
        var tier = guild.PremiumTier.ToString().ToLower().Replace("tier", "").Replace("none", "0");

        var color = guild.GetHighestRole(true)?.Color ?? Color.Default;
        var embed = new EmbedBuilder()
            .WithFooter(Context.User)
            .WithColor(color)
            .WithTitle(guild.Name)
            .WithThumbnailUrl(guild.IconUrl)
            .WithCurrentTimestamp();

        if (!string.IsNullOrEmpty(guild.Description))
            embed.WithDescription(guild.Description.Cut(EmbedBuilder.MaxDescriptionLength, true));

        if (!string.IsNullOrEmpty(guild.BannerId))
            embed.WithImageUrl(guild.BannerUrl);

        var textChannelsCount = guild.TextChannels.Count(o => o is not IThreadChannel);
        var threadChannelsCount = guild.TextChannels.Count(o => o is IThreadChannel);

        embed.AddField(GetText(nameof(GetInfoAsync), "CategoryCount"), guild.CategoryChannels?.Count ?? 0, true)
            .AddField(GetText(nameof(GetInfoAsync), "TextChannelCount"), textChannelsCount, true)
            .AddField(GetText(nameof(GetInfoAsync), "ThreadsCount"), threadChannelsCount, true)
            .AddField(GetText(nameof(GetInfoAsync), "VoiceChannelCount"), guild.VoiceChannels.Count, true)
            .AddField(GetText(nameof(GetInfoAsync), "RoleCount"), guild.Roles.Count, true)
            .AddField(GetText(nameof(GetInfoAsync), "EmoteCount"), $"{basicEmotesCount} / {animatedCount}", true)
            .AddField(GetText(nameof(GetInfoAsync), "BanCount"), banCount, true)
            .AddField(GetText(nameof(GetInfoAsync), "CreatedAt"), guild.CreatedAt.LocalDateTime.ToCzechFormat(), true)
            .AddField(GetText(nameof(GetInfoAsync), "Owner"), guild.Owner.GetFullName())
            .AddField(GetText(nameof(GetInfoAsync), "MemberCount"), guild.MemberCount, true)
            .AddField(GetText(nameof(GetInfoAsync), "PremiumTier"), tier, true)
            .AddField(GetText(nameof(GetInfoAsync), "BoosterCount"), guild.PremiumSubscriptionCount, true);

        if (guild.Features.Value != GuildFeature.None)
        {
            var features = GuildHelper.GetFeatures(guild, Locale, GetTextId(nameof(GetInfoAsync), "Features")).ToList();
            embed.AddField(GetText(nameof(GetInfoAsync), "Improvements"), string.Join("\n", features));
        }

        if (await UserManager.CheckFlagsAsync(Context.User, UserFlags.WebAdmin))
            embed.AddField(GetText(nameof(GetInfoAsync), "DetailsTitle"), GetText(nameof(GetInfoAsync), "DetailsText"));

        await SetResponseAsync(embed: embed.Build());
    }
}
