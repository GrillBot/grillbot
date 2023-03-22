using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Services.Math;
using GrillBot.Common.Services.Math.Models;
using GrillBot.Core.Extensions;

namespace GrillBot.App.Actions.Commands;

public class SolveExpression : CommandAction
{
    private IMathClient MathClient { get; }
    private ITextsManager Texts { get; }

    public SolveExpression(IMathClient mathClient, ITextsManager texts)
    {
        MathClient = mathClient;
        Texts = texts;
    }

    public async Task<Embed> ProcessAsync(string expression)
    {
        var result = await MathClient.SolveExpressionAsync(new MathJsRequest { Expression = expression });

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
        => Texts[$"MathModule/SolveExpression/{id}", Locale];
}
