using ImageMagick;

namespace GrillBot.App.Actions.Commands.Points.Chart;

public static class UsersChartRenderer
{
    public static MagickImage Render(IGuild guild, Dictionary<ulong, List<(DateTime day, int messagePoints, int reactionPoints)>> data, ChartsFilter filter)
    {
        return null; // TODO
    }
}
