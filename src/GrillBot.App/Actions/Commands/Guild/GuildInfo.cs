using GrillBot.App.Managers;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Extensions;
using GrillBot.Database.Enums;

namespace GrillBot.App.Actions.Commands.Guild;

public class GuildInfo : CommandAction
{
    private GuildHelper GuildHelper { get; }
    private UserManager UserManager { get; }
    private ITextsManager Texts { get; }

    public GuildInfo(GuildHelper guildHelper, UserManager userManager, ITextsManager texts)
    {
        GuildHelper = guildHelper;
        UserManager = userManager;
        Texts = texts;
    }

    public async Task<Embed> ProcessAsync()
    {
        var basicEmotesCount = Context.Guild.Emotes.Count(o => !o.Animated);
        var animatedCount = Context.Guild.Emotes.Count - basicEmotesCount;
        var banCount = (await Context.Guild.GetBansAsync().FlattenAsync()).Count();
        var tier = Context.Guild.PremiumTier.ToString().ToLower().Replace("tier", "").Replace("none", "0");
        var color = Context.Guild.GetHighestRole(true)?.Color ?? Color.Default;
        var textChannels = await Context.Guild.GetTextChannelsAsync();
        var textChannelsCount = textChannels.Count(o => o is not IThreadChannel);
        var threadChannelsCount = textChannels.Count(o => o is IThreadChannel);
        var categoryCount = (await Context.Guild.GetCategoriesAsync()).Count;
        var voiceCount = (await Context.Guild.GetVoiceChannelsAsync()).Count;
        var owner = await Context.Guild.GetOwnerAsync();

        var embed = new EmbedBuilder()
            .WithFooter(Context.User)
            .WithColor(color)
            .WithTitle(Context.Guild.Name)
            .WithThumbnailUrl(Context.Guild.IconUrl)
            .WithCurrentTimestamp();

        if (!string.IsNullOrEmpty(Context.Guild.Description))
            embed.WithDescription(Context.Guild.Description.Cut(EmbedBuilder.MaxDescriptionLength, true));

        if (!string.IsNullOrEmpty(Context.Guild.BannerId))
            embed.WithImageUrl(Context.Guild.BannerUrl);

        embed.AddField(GetText("CategoryCount"), categoryCount, true)
            .AddField(GetText("TextChannelCount"), textChannelsCount, true)
            .AddField(GetText("ThreadsCount"), threadChannelsCount, true)
            .AddField(GetText("VoiceChannelCount"), voiceCount, true)
            .AddField(GetText("RoleCount"), Context.Guild.Roles.Count, true)
            .AddField(GetText("EmoteCount"), $"{basicEmotesCount} / {animatedCount}", true)
            .AddField(GetText("BanCount"), banCount, true)
            .AddField(GetText("CreatedAt"), Context.Guild.CreatedAt.LocalDateTime.ToCzechFormat(), true)
            .AddField(GetText("Owner"), owner.GetFullName())
            .AddField(GetText("MemberCount"), Context.Guild.ApproximateMemberCount ?? 0, true)
            .AddField(GetText("PremiumTier"), tier, true)
            .AddField(GetText("BoosterCount"), Context.Guild.PremiumSubscriptionCount, true);

        if (Context.Guild.Features.Value != GuildFeature.None)
        {
            var features = GuildHelper.GetFeatures(Context.Guild, Locale, "GuildModule/GetInfo/Features").ToList();
            embed.AddField(GetText("Improvements"), string.Join("\n", features));
        }

        if (await UserManager.CheckFlagsAsync(Context.User, UserFlags.WebAdmin))
            embed.AddField(GetText("DetailsTitle"), GetText("DetailsText"));

        return embed.Build();
    }

    private string GetText(string id)
        => Texts[$"GuildModule/GetInfo/{id}", Locale];
}
