using Discord.Interactions;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.Data.Models.MathJS;
using System.Net.Http;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
public class MathModule : Infrastructure.InteractionsModuleBase
{
    private IHttpClientFactory HttpClientFactory { get; }

    public MathModule(IHttpClientFactory httpClientFactory)
    {
        HttpClientFactory = httpClientFactory;
    }

    [SlashCommand("solve", "Spočítá matematický výraz.")]
    public async Task SolveExpressionAsync(
        [Summary("vyraz", "Matematický výraz k výpočtu.")]
        string expression
    )
    {
        var client = HttpClientFactory.CreateClient("MathJS");
        var requestJson = JsonConvert.SerializeObject(new MathJSRequest() { Expression = expression });
        using var requestContent = new StringContent(requestJson);

        var embed = new EmbedBuilder()
            .WithFooter(Context.User)
            .WithCurrentTimestamp()
            .AddField("Výraz", $"`{expression.Cut(EmbedFieldBuilder.MaxFieldValueLength)}`", false);

        try
        {
            using var response = await client.PostAsync("", requestContent);
            var responseContent = await response.Content.ReadAsStringAsync();
            var calcResult = JsonConvert.DeserializeObject<MathJSResult>(responseContent);

            if (!response.IsSuccessStatusCode)
            {
                embed.WithColor(Color.Red)
                    .WithTitle("Výpočet se nezdařil")
                    .AddField("Hlášení", calcResult.Error, false);
            }
            else
            {
                embed.WithColor(Color.Green)
                    .AddField("Výsledek", calcResult.Result, false);
            }
        }
        catch (TaskCanceledException)
        {
            embed.WithColor(Color.Red)
                .WithTitle("Vypršel časový limit pro zpracování výrazu");
        }

        await SetResponseAsync(embed: embed.Build());
    }
}
