using GrillBot.App.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;

namespace GrillBot.Tests.App.Helpers
{
    [TestClass]
    public class GraphicsHelpersTests
    {
        [TestMethod]
        public void CreateRectangle_Fill()
        {
            using var bitmap = new Bitmap(100, 100);
            using var graphics = Graphics.FromImage(bitmap);

            GraphicsHelpers.CreateRectangle(graphics, new Rectangle(0, 0, 50, 50), Color.Blue, 10, true);
        }

        [TestMethod]
        public void CreateRectangle_Draw()
        {
            using var bitmap = new Bitmap(100, 100);
            using var graphics = Graphics.FromImage(bitmap);

            GraphicsHelpers.CreateRectangle(graphics, new Rectangle(0, 0, 50, 50), Color.Blue, 10, false);
        }
    }
}
