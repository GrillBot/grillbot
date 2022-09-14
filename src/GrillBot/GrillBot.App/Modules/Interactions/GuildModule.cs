using Discord.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Services.User;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers;
using GrillBot.Database.Enums;

namespace GrillBot.App.Modules.Interactions;

[Group("guild", "Guild management")]
[RequireUserPerms]
[RequireBotPermission(GuildPermission.Administrator)]
public class GuildModule : InteractionsModuleBase
{
    private UserService UserService { get; }
    private GuildHelper GuildHelper { get; }

    public GuildModule(UserService userService, LocalizationManager localization, GuildHelper guildHelper) : base(localization)
    {
        UserService = userService;
        GuildHelper = guildHelper;
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

        embed.AddField(GetLocale(nameof(GetInfoAsync), "CategoryCount"), guild.CategoryChannels?.Count ?? 0, true)
            .AddField(GetLocale(nameof(GetInfoAsync), "TextChannelCount"), guild.TextChannels.Count, true)
            .AddField(GetLocale(nameof(GetInfoAsync), "VoiceChannelCount"), guild.VoiceChannels.Count, true)
            .AddField(GetLocale(nameof(GetInfoAsync), "RoleCount"), guild.Roles.Count, true)
            .AddField(GetLocale(nameof(GetInfoAsync), "EmoteCount"), $"{basicEmotesCount} / {animatedCount}", true)
            .AddField(GetLocale(nameof(GetInfoAsync), "BanCount"), banCount, true)
            .AddField(GetLocale(nameof(GetInfoAsync), "CreatedAt"), guild.CreatedAt.LocalDateTime.ToCzechFormat(), true)
            .AddField(GetLocale(nameof(GetInfoAsync), "Owner"), guild.Owner.GetFullName())
            .AddField(GetLocale(nameof(GetInfoAsync), "MemberCount"), guild.MemberCount, true)
            .AddField(GetLocale(nameof(GetInfoAsync), "PremiumTier"), tier, true)
            .AddField(GetLocale(nameof(GetInfoAsync), "BoosterCount"), guild.PremiumSubscriptionCount, true);

        if (guild.Features.Value != GuildFeature.None)
        {
            var features = GuildHelper.GetFeatures(guild, Context.Interaction.UserLocale, GetLocaleId(nameof(GetInfoAsync), "Features")).ToList();
            embed.AddField(GetLocale(nameof(GetInfoAsync), "Improvements"), string.Join("\n", features));
        }

        if (await UserService.CheckUserFlagsAsync(Context.User, UserFlags.WebAdmin))
            embed.AddField(GetLocale(nameof(GetInfoAsync), "DetailsTitle"), GetLocale(nameof(GetInfoAsync), "DetailsText"));

        await SetResponseAsync(embed: embed.Build());
    }
}
