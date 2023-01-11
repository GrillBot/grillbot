using GrillBot.App.Services.Graphics;
using GrillBot.App.Services.Graphics.Models.Chart;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Localization;
using ImageMagick;

namespace GrillBot.App.Actions.Commands.Points.Chart;

public class UsersChartRenderer
{
    private ITextsManager Texts { get; }
    private IGraphicsClient GraphicsClient { get; }
    private RandomizationManager RandomizationManager { get; }
    private string Locale { get; }

    public UsersChartRenderer(ITextsManager texts, IGraphicsClient graphicsClient, RandomizationManager randomizationManager, string locale)
    {
        Texts = texts;
        GraphicsClient = graphicsClient;
        Locale = locale;
        RandomizationManager = randomizationManager;
    }

    public async Task<MagickImage> RenderAsync(IGuild guild, Dictionary<ulong, List<(DateTime day, int messagePoints, int reactionPoints)>> data, ChartsFilter filter)
    {
        var request = await CreateRequestAsync(guild, data, filter);
        var chartData = await GraphicsClient.CreateChartAsync(request);

        return new MagickImage(chartData);
    }

    private async Task<ChartRequestData> CreateRequestAsync(IGuild guild, Dictionary<ulong, List<(DateTime day, int messagePoints, int reactionPoints)>> data, ChartsFilter filter)
    {
        var request = ChartRequestBuilder.CreateCommonRequest();

        request.Data.TopLabel!.Text = filter switch
        {
            ChartsFilter.Messages => Texts["Points/Chart/Title/User/Messages", Locale],
            ChartsFilter.Reactions => Texts["Points/Chart/Title/User/Reactions", Locale],
            _ => Texts["Points/Chart/Title/User/Summary", Locale]
        };

        var dates = ChartRequestBuilder.FilterData(
            data.SelectMany(o => o.Value).GroupBy(o => o.day).Select(o => (o.Key, o.Sum(x => x.messagePoints), o.Sum(x => x.reactionPoints))),
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
                    Label = day.ToCzechFormat(true),
                    Value = filtered.Where(x => x.day <= day).Sum(x => x.points)
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
                RandomizationManager.GetNext("PointsGraph", 255),
                RandomizationManager.GetNext("PointsGraph", 255),
                RandomizationManager.GetNext("PointsGraph", 255)
            );
        }

        usedColors.Add(color.Value.RawValue);
        return color.Value;
    }
}
