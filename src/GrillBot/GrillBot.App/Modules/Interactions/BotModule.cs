using Discord.Interactions;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Services.Unverify;
using System.Diagnostics;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
[Group("bot", "Příkazy k informacím a konfiguraci bota.")]
public class BotModule : Infrastructure.InteractionsModuleBase
{
    [SlashCommand("info", "Informace o botovi")]
    public async Task BotInfoAsync()
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

        await SetResponseAsync(embed: embed);
    }

    [Group("selfunverify", "Konfigurace selfunverify.")]
    public class SelfUnverifyConfig : Infrastructure.InteractionsModuleBase
    {
        private SelfunverifyService Service { get; }

        public SelfUnverifyConfig(SelfunverifyService service)
        {
            Service = service;
        }

        [SlashCommand("list-keepables", "Seznam ponechatelných přístpů při selfunverify")]
        public async Task ListAsync(string group = null)
        {
            var data = await Service.GetKeepablesAsync(group);

            if (data.Count == 0)
            {
                await SetResponseAsync("Nebyly nalezeny žádné ponechatelné přístupy.");
                return;
            }

            var embed = new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithCurrentTimestamp()
                .WithFooter(Context.User)
                .WithTitle("Ponechatelné role a kanály");

            foreach (var grp in data.GroupBy(o => string.Join("|", o.Value)))
            {
                var keys = string.Join(", ", grp.Select(o => o.Key == "_" ? "Ostatní" : o.Key));

                var fieldGroupBuilder = new StringBuilder();
                foreach (var item in grp.First().Value)
                {
                    if (fieldGroupBuilder.Length + item.Length >= EmbedFieldBuilder.MaxFieldValueLength)
                    {
                        var fieldGroupResult = fieldGroupBuilder.ToString().Trim();
                        embed.AddField(keys, fieldGroupResult.EndsWith(",") ? fieldGroupResult[0..^1] : fieldGroupResult, false);
                        fieldGroupBuilder.Clear();
                    }
                    else
                    {
                        fieldGroupBuilder.Append(item).Append(", ");
                    }
                }

                if (fieldGroupBuilder.Length > 0)
                {
                    var fieldGroupResult = fieldGroupBuilder.ToString().Trim();
                    embed.AddField(keys, fieldGroupResult.EndsWith(",") ? fieldGroupResult[0..^1] : fieldGroupResult, false);
                    fieldGroupBuilder.Clear();
                }
            }

            await SetResponseAsync(embed: embed.Build());
        }
    }
}
