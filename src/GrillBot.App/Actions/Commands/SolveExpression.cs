using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Services.Math;
using GrillBot.Common.Services.Math.Models;
using GrillBot.Core.Extensions;
using GrillBot.Core.Services.Common.Exceptions;
using GrillBot.Core.Services.Common.Executor;

namespace GrillBot.App.Actions.Commands;

public class SolveExpression(
    IServiceClientExecutor<IMathClient> _client,
    ITextsManager _texts
) : CommandAction
{
    public async Task<Embed> ProcessAsync(string expression)
    {
        var result = await SolveExpressionAsync(expression);

        var embed = new EmbedBuilder()
            .WithFooter(Context.User)
            .WithCurrentTimestamp()
            .AddField(GetText("Expression"), $"`{expression.Cut(EmbedFieldBuilder.MaxFieldValueLength - 2)}`");

        if (!string.IsNullOrEmpty(result.Error))
        {
            embed.WithColor(Color.Red)
                .WithTitle(GetText("ComputeFailed"))
                .AddField(GetText("Report"), result.Error);
        }
        else if (result.IsTimeout)
        {
            embed
                .WithColor(Color.Red)
                .WithTitle(GetText("Timeout"));
        }
        else
        {
            embed.WithColor(Color.Green)
                .AddField(GetText("Result"), result.Result);
        }

        return embed.Build();
    }

    private string GetText(string id)
        => _texts[$"MathModule/SolveExpression/{id}", Locale];

    private async Task<MathJsResult> SolveExpressionAsync(string expression)
    {
        try
        {
            var request = new MathJsRequest { Expression = expression };
            return await _client.ExecuteRequestAsync((c, ctx) => c.SolveExpressionAsync(request, ctx.CancellationToken));
        }
        catch (ClientBadRequestException ex) when (!string.IsNullOrEmpty(ex.RawData))
        {
            return JsonConvert.DeserializeObject<MathJsResult>(ex.RawData)!;
        }
        catch (Exception ex) when (ex is TaskCanceledException or TimeoutException)
        {
            return new MathJsResult { IsTimeout = true };
        }
    }
}
