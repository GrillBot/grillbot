using Diag = System.Diagnostics;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;

namespace GrillBot.App.Actions.Commands;

public class BotInfo : CommandAction
{
    private ITextsManager Texts { get; }
    private CultureInfo Culture => Texts.GetCulture(Locale);

    public BotInfo(ITextsManager texts)
    {
        Texts = texts;
    }

    public async Task<Embed> ProcessAsync()
    {
        var process = Diag.Process.GetCurrentProcess();
        var botUser = await GetCurrentUserAsync();
        var color = botUser.GetHighestRole(true)?.Color ?? Color.Default;

        return new EmbedBuilder()
            .WithTitle(botUser.GetFullName())
            .WithThumbnailUrl(botUser.GetUserAvatarUrl())
            .AddField(GetText("CreatedAt"), botUser.CreatedAt.LocalDateTime.Humanize(culture: Culture))
            .AddField(GetText("Uptime"), (DateTime.Now - process.StartTime).Humanize(culture: Culture, maxUnit: TimeUnit.Day))
            .AddField(GetText("Repository"), "https://github.com/grillbot")
            .AddField(GetText("Documentation"), "https://docs.grillbot.eu/")
            .AddField(GetText("Swagger"), "https://grillbot.eu/swagger")
            .AddField(GetText("NavigationPage"), "https://grillbot.eu")
            .AddField(GetText("PrivateAdmin"), "https://private.grillbot.eu")
            .AddField(GetText("PublicAdmin"), "https://public.grillbot.eu/")
            .WithColor(color)
            .WithCurrentTimestamp()
            .WithFooter(Context.User)
            .Build();
    }

    private string GetText(string id)
        => Texts[$"BotModule/BotInfo/{id}", Locale];
}
