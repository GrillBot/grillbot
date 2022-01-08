using Discord.Commands;
using GrillBot.App.Infrastructure.TypeReaders.TextBased;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace GrillBot.Tests.App.Infrastructure.TypeReaders.TextBased
{
    [TestClass]
    public class BooleanTypeReaderTests
    {
        [TestMethod]
        public void Read_BasicBoolean()
        {
            var cases = new Dictionary<string, bool>()
            {
                { "True", true },
                { "False", false },
                { "true", true },
                { "false", false }
            };

            var context = new Mock<ICommandContext>();
            var reader = new BooleanTypeReader();
            foreach (var @case in cases)
            {
                var result = reader.ReadAsync(context.Object, @case.Key, null).Result;

                Assert.IsTrue(result.IsSuccess);
                Assert.AreEqual(1, result.Values.Count);
                Assert.AreEqual(@case.Value, result.Values.First().Value);
            }
        }

        [TestMethod]
        public void Read_Regex()
        {
            var cases = new Dictionary<string, bool>()
            {
                { "ano", true },
                { "ne", false },
                { "no", false },
                { "yes", true },
                { "tru", true },
                { "fals", false }
            };

            var context = new Mock<ICommandContext>();
            var reader = new BooleanTypeReader();
            foreach (var @case in cases)
            {
                var result = reader.ReadAsync(context.Object, @case.Key, null).Result;

                Assert.IsTrue(result.IsSuccess);
                Assert.AreEqual(1, result.Values.Count);
                Assert.AreEqual(@case.Value, result.Values.First().Value);
            }
        }

        [TestMethod]
        public void Read_Invalid()
        {
            var context = new Mock<ICommandContext>();
            var reader = new BooleanTypeReader();
            var result = reader.ReadAsync(context.Object, "nikdy", null).Result;

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(CommandError.ParseFailed, result.Error);
        }
    }
}
