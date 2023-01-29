using Discord.Interactions;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.App.Infrastructure.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
[Group("bot", "Bot information and configuration commands.")]
public class BotModule : InteractionsModuleBase
{
    public BotModule(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [SlashCommand("info", "Bot info")]
    public async Task BotInfoAsync()
    {
        using var command = GetCommand<Actions.Commands.BotInfo>();
        var embed = await command.Command.ProcessAsync();

        await SetResponseAsync(embed: embed);
    }

    [Group("selfunverify", "Configuring selfunverify.")]
    public class SelfUnverifyConfig : InteractionsModuleBase
    {
        public SelfUnverifyConfig(IServiceProvider serviceProvider) : base(serviceProvider)
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
                await SetResponseAsync(Texts["Unverify/SelfUnverify/Keepables/List/NoKeepables", Locale]);
                return;
            }

            var embed = new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithCurrentTimestamp()
                .WithFooter(Context.User)
                .WithTitle(Texts["Unverify/SelfUnverify/Keepables/List/Title", Locale]);

            foreach (var grp in data.GroupBy(o => string.Join("|", o.Value)))
            {
                string fieldGroupResult;
                var keys = string.Join(", ", grp.Select(o => o.Key == "_" ? Texts["Unverify/SelfUnverify/Keepables/List/Other", Locale] : o.Key));

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
