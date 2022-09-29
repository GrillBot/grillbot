using ImageMagick;

namespace GrillBot.Tests.App;

[TestClass]
public class RandomTest
{
    [TestMethod]
    public void OpenSans()
    {
        using var img = new MagickImage(MagickColors.White, 500, 500);
        new Drawables().Font("Open Sans").Text(100,100, "Text").Draw(img);
    }

    [TestMethod]
    public void Arial()
    {
        using var img = new MagickImage(MagickColors.White, 500, 500);
        new Drawables().Font("Arial").Text(100,100, "Text").Draw(img);
    }
}
