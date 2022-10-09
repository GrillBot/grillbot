using System;
using System.Text;

namespace GrillBot.Data.Models;

/// <summary>
/// Text based ProgressBar.
/// </summary>
/// <remarks>Credits to https://github.com/solumath</remarks>
public class ProgressBar
{
    private const int MaxBarLength = 10;
    private const char EmptyBarItem = '░';
    private const char FilledBarItem = '▓';

    private int Total { get; }
    private int Current { get; set; }
    private string AdditionalContent { get; set; }

    public double Percentage => Math.Round(Current / (double)Total, 1);

    public ProgressBar(int total)
    {
        Total = total;
    }

    public void SetValue(int value, string additionalContent)
    {
        Current = value;
        AdditionalContent = additionalContent;
    }

    public override string ToString()
    {
        var builder = new StringBuilder();

        if (Total > 0)
        {
            for (var i = 0; i < MaxBarLength; i++)
                builder.Append(Percentage <= 1.0D / MaxBarLength * i ? EmptyBarItem : FilledBarItem);
        }
        else
        {
            builder.Append(new string(EmptyBarItem, MaxBarLength));
        }

        return builder
            .Append($" ({Math.Round(Percentage * 100)} %) {AdditionalContent}")
            .ToString();
    }

    public bool ValueChanged(int lastPercentage) => (int)Math.Round(Percentage * 100) != lastPercentage;
}
