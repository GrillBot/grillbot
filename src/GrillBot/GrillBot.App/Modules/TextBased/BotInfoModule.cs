using Discord;
using Discord.Commands;
using GrillBot.App.Extensions.Discord;
using GrillBot.Data.Extensions.Discord;
using Humanizer;
using Humanizer.Localisation;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

namespace GrillBot.App.Modules.TextBased;

[Name("Obecné informace o botovi")]
public class BotInfoModule : Infrastructure.ModuleBase
{
    [Command("bot")]
    [Alias("about", "o")]
    public Task BotInfoAsync()
    {
        var culture = new CultureInfo("cs-CZ");
        var process = Process.GetCurrentProcess();
        var color = Context.Guild == null
            ? Color.Default
            : Context.Guild.CurrentUser.GetHighestRole(true)?.Color ?? Color.Default;
        var user = (IUser)Context.Guild?.CurrentUser ?? Context.Client.CurrentUser;

        var embed = new EmbedBuilder()
            .WithTitle(user.GetFullName())
            .WithThumbnailUrl(user.GetAvatarUri())
            .AddField("Založen", user.CreatedAt.LocalDateTime.Humanize(culture: culture))
            .AddField("Uptime", (DateTime.Now - process.StartTime).Humanize(culture: culture, maxUnit: TimeUnit.Day))
            .AddField("Repozitář", "https://gitlab.com/grillbot")
            .AddField("Dokumentace", "https://docs.grillbot.cloud/")
            .AddField("Swagger", "https://grillbot.cloud/swagger")
            .AddField("Administrace (Vyšší oprávnění)", "https://grillbot.cloud")
            .AddField("Administrace (Pro ostatní)", "https://public.grillbot.cloud/")
            .WithColor(color)
            .WithCurrentTimestamp()
            .WithFooter(Context.User)
            .Build();

        return ReplyAsync(embed: embed);
    }
}
