using Discord.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Services.User;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Database.Enums;

namespace GrillBot.App.Modules.Interactions;

[Group("guild", "Management serveru")]
[RequireUserPerms]
[RequireBotPermission(GuildPermission.Administrator)]
public class GuildModule : InteractionsModuleBase
{
    private UserService UserService { get; }

    public GuildModule(UserService userService)
    {
        UserService = userService;
    }

    [SlashCommand("info", "Informace o serveru")]
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

        embed.AddField("Počet kategorií", guild.CategoryChannels?.Count ?? 0, true)
            .AddField("Počet textových kanálů", guild.TextChannels.Count, true)
            .AddField("Počet hlasových kanálů", guild.VoiceChannels.Count, true)
            .AddField("Počet rolí", guild.Roles.Count, true)
            .AddField("Počet emotů (běžných/animovaných)", $"{basicEmotesCount} / {animatedCount}", true)
            .AddField("Počet banů", banCount, true)
            .AddField("Vytvořen", guild.CreatedAt.LocalDateTime.ToCzechFormat(), true)
            .AddField("Vlastník", guild.Owner.GetFullName())
            .AddField("Počet členů", guild.Users.Count, true)
            .AddField("Úroveň serveru", tier, true)
            .AddField("Počet boosterů", guild.PremiumSubscriptionCount, true);

        if (guild.Features.Value != GuildFeature.None)
            embed.AddField("Vylepšení", string.Join("\n", guild.GetTranslatedFeatures()));

        if (await UserService.CheckUserFlagsAsync(Context.User, UserFlags.WebAdmin))
            embed.AddField("Podrobnosti", "Podrobné informace o serveru najdeš ve webové administraci (https://grillbot.cloud/)");

        await SetResponseAsync(embed: embed.Build());
    }
}
