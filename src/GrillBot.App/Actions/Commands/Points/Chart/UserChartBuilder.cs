using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Services.Graphics.Models.Chart;
using GrillBot.Core.Services.PointsService.Models;
using GrillBot.Core.Managers.Random;

namespace GrillBot.App.Actions.Commands.Points.Chart;

public class UserChartBuilder : ChartBuilderBase<Dictionary<ulong, List<PointsChartItem>>>
{
    private IRandomManager RandomManager { get; }

    public UserChartBuilder(ITextsManager texts, IRandomManager randomManager) : base(texts)
    {
        RandomManager = randomManager;
    }

    protected override string CreateTopLabel(ChartsFilter filter, string locale)
    {
        return filter switch
        {
            ChartsFilter.Messages => Texts["Points/Chart/Title/User/Messages", locale],
            ChartsFilter.Reactions => Texts["Points/Chart/Title/User/Reactions", locale],
            _ => Texts["Points/Chart/Title/User/Summary", locale]
        };
    }

    protected override async IAsyncEnumerable<Dataset> CreateDatasetsAsync(ChartsFilter filter)
    {
        var dates = ChartRequestBuilder.FilterData(
            Data.SelectMany(o => o.Value).GroupBy(o => o.Day).SelectMany(o => o.ToList()),
            filter
        ).Select(o => o.day).OrderBy(o => o).ToList();
        var usedColors = new HashSet<uint>();

        foreach (var (userId, userData) in Data)
        {
            var guildUser = await Guild.GetUserAsync(userId);
            if (guildUser is null) continue;

            var filtered = ChartRequestBuilder.FilterData(userData, filter);
            yield return new Dataset
            {
                Data = dates.ConvertAll(day => new DataPoint
                {
                    Label = day.ToCzechFormat(),
                    Value = (int?)filtered.Where(x => x.day <= day).Sum(x => x.points)
                }),
                Color = CreateColor(guildUser, usedColors).ToString(),
                Label = guildUser.GetFullName(),
                Width = 1
            };
        }
    }

    private Color CreateColor(IGuildUser user, ICollection<uint> usedColors)
    {
        var rolesWithColor = user.GetRoles()
            .Where(o => o.Color != Color.Default && !usedColors.Contains(o.Color.RawValue)) // Find non used roles.
            .ToList();

        var color = rolesWithColor.MaxBy(x => x.Position)?.Color;
        if (color == null)
        {
            // User not usable role. Create random color.
            return new Color(
                RandomManager.GetNext("PointsGraph", 255),
                RandomManager.GetNext("PointsGraph", 255),
                RandomManager.GetNext("PointsGraph", 255)
            );
        }

        usedColors.Add(color.Value.RawValue);
        return color.Value;
    }
}
