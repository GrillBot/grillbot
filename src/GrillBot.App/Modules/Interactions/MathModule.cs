using Discord.Interactions;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.Data.Models.MathJS;
using System.Net.Http;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
public class MathModule : InteractionsModuleBase
{
    private IHttpClientFactory HttpClientFactory { get; }

    public MathModule(IHttpClientFactory httpClientFactory, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        HttpClientFactory = httpClientFactory;
    }

    [SlashCommand("solve", "Calculates a mathematical expression.")]
    public async Task SolveExpressionAsync(
        [Summary("expression", "Mathematical expression to calculate.")]
        string expression
    )
    {
        var client = HttpClientFactory.CreateClient("MathJS");
        var requestJson = JsonConvert.SerializeObject(new MathJsRequest { Expression = expression });
        using var requestContent = new StringContent(requestJson);

        var embed = new EmbedBuilder()
            .WithFooter(Context.User)
            .WithCurrentTimestamp()
            .AddField(GetText(nameof(SolveExpressionAsync), "Expression"), $"`{expression.Cut(EmbedFieldBuilder.MaxFieldValueLength - 2)}`");

        try
        {
            using var response = await client.PostAsync("", requestContent);
            var responseContent = await response.Content.ReadAsStringAsync();
            var calcResult = JsonConvert.DeserializeObject<MathJsResult>(responseContent)!;

            if (!response.IsSuccessStatusCode)
            {
                embed.WithColor(Color.Red)
                    .WithTitle(GetText(nameof(SolveExpressionAsync), "ComputeFailed"))
                    .AddField(GetText(nameof(SolveExpressionAsync), "Report"), calcResult.Error);
            }
            else
            {
                embed.WithColor(Color.Green)
                    .AddField(GetText(nameof(SolveExpressionAsync), "Result"), calcResult.Result);
            }
        }
        catch (TaskCanceledException)
        {
            embed.WithColor(Color.Red)
                .WithTitle(GetText(nameof(SolveExpressionAsync), "Timeout"));
        }

        await SetResponseAsync(embed: embed.Build());
    }
}
