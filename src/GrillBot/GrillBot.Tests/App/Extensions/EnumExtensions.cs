using GrillBot.Data.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;

namespace GrillBot.Tests.App.Extensions
{
    [TestClass]
    public class EnumExtensions
    {
        [TestMethod]
        public void GetAttribute_Without()
        {
            const SomeEnum enumItem = SomeEnum.A;
            var attribute = enumItem.GetAttribute<LocalizableAttribute>();

            Assert.IsNull(attribute);
        }

        [TestMethod]
        public void GetAttribute_With()
        {
            const SomeEnum enumItem = SomeEnum.Z;
            var attribute = enumItem.GetAttribute<LocalizableAttribute>();

            Assert.IsNotNull(attribute);
            Assert.IsTrue(attribute.IsLocalizable);
        }

        [TestMethod]
        public void GetDescription_Without()
        {
            const SomeEnum enumItem = SomeEnum.Z;
            var attribute = enumItem.GetDescription();

            Assert.IsNull(attribute);
        }

        [TestMethod]
        public void GetDescription_WithoutText()
        {
            const SomeEnum enumItem = SomeEnum.Y;
            var attribute = enumItem.GetDescription();

            Assert.IsTrue(string.IsNullOrEmpty(attribute));
        }

        [TestMethod]
        public void GetDescription_WithText()
        {
            const SomeEnum enumItem = SomeEnum.X;
            var attribute = enumItem.GetDescription();

            Assert.IsFalse(string.IsNullOrEmpty(attribute));
        }

        [TestMethod]
        public void GetDisplayName()
        {
            Assert.AreEqual("ABCD", SomeEnum.T.GetDisplayName());
        }
    }
}
