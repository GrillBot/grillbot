using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Services.Graphics;
using GrillBot.Common.Services.Graphics.Models.Chart;
using GrillBot.Common.Services.PointsService.Models;
using GrillBot.Core.Managers.Random;
using ImageMagick;

namespace GrillBot.App.Actions.Commands.Points.Chart;

public class UsersChartRenderer
{
    private ITextsManager Texts { get; }
    private IGraphicsClient GraphicsClient { get; }
    private IRandomManager RandomManager { get; }

    public UsersChartRenderer(ITextsManager texts, IGraphicsClient graphicsClient, IRandomManager randomManager)
    {
        Texts = texts;
        GraphicsClient = graphicsClient;
        RandomManager = randomManager;
    }

    public async Task<MagickImage> RenderAsync(IGuild guild, Dictionary<ulong, List<PointsChartItem>> data, ChartsFilter filter, string locale)
    {
        var request = await CreateRequestAsync(guild, data, filter, locale);
        var chartData = await GraphicsClient.CreateChartAsync(request);

        return new MagickImage(chartData);
    }

    private async Task<ChartRequestData> CreateRequestAsync(IGuild guild, Dictionary<ulong, List<PointsChartItem>> data, ChartsFilter filter, string locale)
    {
        var request = ChartRequestBuilder.CreateCommonRequest();

        request.Data.TopLabel!.Text = filter switch
        {
            ChartsFilter.Messages => Texts["Points/Chart/Title/User/Messages", locale],
            ChartsFilter.Reactions => Texts["Points/Chart/Title/User/Reactions", locale],
            _ => Texts["Points/Chart/Title/User/Summary", locale]
        };

        var dates = ChartRequestBuilder.FilterData(
            data.SelectMany(o => o.Value).GroupBy(o => o.Day).SelectMany(o => o.ToList()),
            filter
        ).Select(x => x.day).OrderBy(x => x).ToList();
        var usedColors = new List<uint>();

        foreach (var (userId, userData) in data)
        {
            var guildUser = await guild.GetUserAsync(userId);
            if (guildUser == null) continue;

            var filtered = ChartRequestBuilder.FilterData(userData, filter);
            request.Data.Datasets.Add(new Dataset
            {
                Data = dates.ConvertAll(day => new DataPoint
                {
                    Label = day.ToCzechFormat(),
                    Value = (int?)filtered.Where(x => x.day <= day).Sum(x => x.points)
                }),
                Width = 1,
                Color = CreateColor(guildUser, usedColors).ToString(),
                Label = guildUser.GetFullName()
            });
        }

        return request;
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
