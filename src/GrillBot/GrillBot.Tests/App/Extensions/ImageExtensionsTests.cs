using GrillBot.App.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Extensions
{
    [TestClass]
    public class ImageExtensionsTests
    {
        [TestMethod]
        public void RoundImage()
        {
            using var image = new Bitmap(100, 100);
            using var rounded = image.RoundImage();

            Assert.IsNotNull(rounded);
        }

        [TestMethod]
        public void ResizeImage()
        {
            using var image = new Bitmap(100, 100);
            using var resized = image.ResizeImage(50, 50);

            Assert.AreEqual(50, resized.Width);
            Assert.AreEqual(50, resized.Height);
        }

        [TestMethod]
        public void ResizeImage_Size()
        {
            using var image = new Bitmap(100, 100);
            using var resized = image.ResizeImage(new Size(50, 50));

            Assert.AreEqual(50, resized.Width);
            Assert.AreEqual(50, resized.Height);
        }

        [TestMethod]
        public void CropImage()
        {
            using var bitmap = new Bitmap(150, 150);
            using var cropped = bitmap.CropImage(new Rectangle(0, 0, 50, 50));

            Assert.AreEqual(50, cropped.Width);
            Assert.AreEqual(50, cropped.Height);
        }

        [TestMethod]
        public void SplitGifIntoFrames()
        {
            using var image = new Bitmap(100, 100);
            var frames = image.SplitGifIntoFrames().ToList();

            Assert.AreEqual(1, frames.Count);
        }

        [TestMethod]
        public void CalculateGifDelay()
        {
            using var image = new Bitmap(100, 100);

            var property = (PropertyItem)FormatterServices.GetUninitializedObject(typeof(PropertyItem));
            property.Id = 0x5100;
            property.Value = new byte[] { 5, 10 };
            image.SetPropertyItem(property);

            var delay = image.CalculateGifDelay();
            Assert.AreEqual(2565, delay);
        }
    }
}
