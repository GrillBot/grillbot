using Discord.Interactions;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.Common.Extensions.Discord;
using System.Diagnostics;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.Common.Managers.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
[Group("bot", "Bot information and configuration commands.")]
public class BotModule : InteractionsModuleBase
{
    public BotModule(ITextsManager texts) : base(texts)
    {
    }

    [SlashCommand("info", "Bot info")]
    public async Task BotInfoAsync()
    {
        var process = Process.GetCurrentProcess();
        var color = Context.Guild == null
            ? Color.Default
            : Context.Guild.CurrentUser.GetHighestRole(true)?.Color ?? Color.Default;
        var user = (IUser)Context.Guild?.CurrentUser ?? Context.Client.CurrentUser;

        var embed = new EmbedBuilder()
            .WithTitle(user.GetFullName())
            .WithThumbnailUrl(user.GetUserAvatarUrl())
            .AddField(GetText(nameof(BotInfoAsync), "CreatedAt"), user.CreatedAt.LocalDateTime.Humanize(culture: Culture))
            .AddField(GetText(nameof(BotInfoAsync), "Uptime"), (DateTime.Now - process.StartTime).Humanize(culture: Culture, maxUnit: TimeUnit.Day))
            .AddField(GetText(nameof(BotInfoAsync), "Repository"), "https://gitlab.com/grillbot")
            .AddField(GetText(nameof(BotInfoAsync), "Documentation"), "https://docs.grillbot.cloud/")
            .AddField(GetText(nameof(BotInfoAsync), "Swagger"), "https://grillbot.cloud/swagger")
            .AddField(GetText(nameof(BotInfoAsync), "PrivateAdmin"), "https://grillbot.cloud")
            .AddField(GetText(nameof(BotInfoAsync), "PublicAdmin"), "https://public.grillbot.cloud/")
            .WithColor(color)
            .WithCurrentTimestamp()
            .WithFooter(Context.User)
            .Build();

        await SetResponseAsync(embed: embed);
    }

    [Group("selfunverify", "Configuring selfunverify.")]
    public class SelfUnverifyConfig : InteractionsModuleBase
    {
        public SelfUnverifyConfig(ITextsManager texts, IServiceProvider serviceProvider) : base(texts, serviceProvider)
        {
        }

        [SlashCommand("list-keepables", "List of allowable accesses when selfunverify")]
        public async Task ListAsync(string group = null)
        {
            using var scope = ServiceProvider.CreateScope();
            var action = scope.ServiceProvider.GetRequiredService<Actions.Api.V1.Unverify.GetKeepablesList>();
            action.UpdateContext(Locale, Context.User);
            var data = await action.ProcessAsync(group);

            if (data.Count == 0)
            {
                await SetResponseAsync(GetText(nameof(ListAsync), "NoKeepables"));
                return;
            }

            var embed = new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithCurrentTimestamp()
                .WithFooter(Context.User)
                .WithTitle(GetText(nameof(ListAsync), "Title"));

            foreach (var grp in data.GroupBy(o => string.Join("|", o.Value)))
            {
                string fieldGroupResult;
                var keys = string.Join(", ", grp.Select(o => o.Key == "_" ? GetText(nameof(ListAsync), "Other") : o.Key));

                var fieldGroupBuilder = new StringBuilder();
                foreach (var item in grp.First().Value)
                {
                    if (fieldGroupBuilder.Length + item.Length >= EmbedFieldBuilder.MaxFieldValueLength)
                    {
                        fieldGroupResult = fieldGroupBuilder.ToString().Trim();
                        embed.AddField(keys, fieldGroupResult.EndsWith(",") ? fieldGroupResult[..^1] : fieldGroupResult);
                        fieldGroupBuilder.Clear();
                    }
                    else
                    {
                        fieldGroupBuilder.Append(item).Append(", ");
                    }
                }

                if (fieldGroupBuilder.Length <= 0)
                    continue;

                fieldGroupResult = fieldGroupBuilder.ToString().Trim();
                embed.AddField(keys, fieldGroupResult.EndsWith(",") ? fieldGroupResult[..^1] : fieldGroupResult);
                fieldGroupBuilder.Clear();
            }

            await SetResponseAsync(embed: embed.Build());
        }
    }
}
