using System.Drawing;
using System.Drawing.Drawing2D;

namespace GrillBot.App.Helpers
{
    static public class GraphicsHelpers
    {
        static public void CreateRectangle(Graphics graphics, Rectangle rectangle, Color color, int radius = 1, bool fill = true)
        {
            using var path = new GraphicsPath();

            path.AddArc(rectangle.X, rectangle.Y, radius * 2, radius * 2, 180, 90);
            path.AddLine(rectangle.X + radius, rectangle.Y, rectangle.X + rectangle.Width - radius, rectangle.Y);
            path.AddArc(rectangle.X + rectangle.Width - (2 * radius), rectangle.Y, 2 * radius, 2 * radius, 270, 90);
            path.AddLine(rectangle.X + rectangle.Width, rectangle.Y + radius, rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height - radius);
            path.AddArc(rectangle.X + rectangle.Width - (2 * radius), rectangle.Y + rectangle.Height - (2 * radius), radius + radius, radius + radius, 0, 91);
            path.AddLine(rectangle.X + radius, rectangle.Y + rectangle.Height, rectangle.X + rectangle.Width - radius, rectangle.Y + rectangle.Height);
            path.AddArc(rectangle.X, rectangle.Y + rectangle.Height - (2 * radius), 2 * radius, 2 * radius, 90, 91);

            path.CloseFigure();

            if (fill)
            {
                using var brush = new SolidBrush(color);
                graphics.FillPath(brush, path);
            }
            else
            {
                using var pen = new Pen(color);
                graphics.DrawPath(pen, path);
            }
        }
    }
}
