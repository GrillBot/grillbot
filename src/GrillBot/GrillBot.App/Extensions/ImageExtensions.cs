using ImageMagick;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace GrillBot.App.Extensions
{
    static public class ImageExtensions
    {
        static public Bitmap RoundImage(this Image original)
        {
            using var brush = new TextureBrush(original);

            var rounded = new Bitmap(original.Width, original.Height, original.PixelFormat);
            rounded.MakeTransparent();

            using var g = Graphics.FromImage(rounded);
            using var gp = new GraphicsPath();

            g.Clear(Color.Transparent);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            gp.AddEllipse(0, 0, original.Width, original.Height);
            g.FillPath(brush, gp);

            return rounded;
        }

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

        /// <summary>
        /// Resizes image
        /// </summary>
        /// <remarks>https://stackoverflow.com/a/24199315</remarks>
        static public Image ResizeImage(this Image original, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            using (var graphics = Graphics.FromImage(destImage))
            {
                using var wrapMode = new ImageAttributes();
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(original, destRect, 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, wrapMode);
            }

            return destImage;
        }

        static public IEnumerable<Image> SplitGifIntoFrames(this Image image)
        {
            for (int i = 0; i < image.GetFrameCount(FrameDimension.Time); i++)
            {
                image.SelectActiveFrame(FrameDimension.Time, i);
                yield return new Bitmap(image);
            }
        }

        static public int CalculateGifDelay(this Image image)
        {
            var item = image.GetPropertyItem(0x5100); // FrameDelay in libgdi+.
            return item.Value[0] + (item.Value[1] * 256);
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
