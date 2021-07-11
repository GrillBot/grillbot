using ImageMagick;
using System.Drawing;
using System.Linq;
using System.Text;

namespace GrillBot.App.Extensions
{
    static public class ImageExtensions
    {
        // Credits to https://github.com/janch32
        static public void RoundImage(this IMagickImage<byte> image)
        {
            using var mask = new MagickImage(MagickColors.Transparent, image.Width, image.Height);

            new Drawables()
                .FillColor(MagickColors.White)
                .Circle(image.Width / (double)2, image.Height / (double)2, image.Width / (double)2, 0)
                .Draw(mask);

            image.Alpha(AlphaOption.On);
            image.Composite(mask, CompositeOperator.Multiply);
        }

        static public MagickColor GetDominantColor(this MagickImage image)
        {
            var histogram = image.Histogram();
            return new(histogram.Aggregate((x, y) => x.Value > y.Value ? x : y).Key);
        }

        static public MagickColor CreateDarkerBackgroundColor(this MagickColor color)
        {
            var tmpColor = Color.FromArgb(color.A, color.R, color.G, color.B);
            if (tmpColor.GetBrightness() <= 0.2)
                tmpColor = Color.FromArgb(25, Color.White);
            else
                tmpColor = Color.FromArgb(100, Color.Black);

            return MagickColor.FromRgba(tmpColor.R, tmpColor.G, tmpColor.B, tmpColor.A);
        }

        static public string CutToImageWidth(this string str, int width, string font, double fontSize)
        {
            var drawables = new Drawables()
                .Font(font)
                .FontPointSize(fontSize);

            var builder = new StringBuilder();
            foreach (var character in str)
            {
                var metrics = drawables.FontTypeMetrics($"{builder}{character}");
                if (metrics.TextWidth >= width) break;
                builder.Append(character);
            }

            return builder.ToString();
        }
    }
}
