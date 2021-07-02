using Discord.Commands;
using GrillBot.App.Infrastructure.TypeReaders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace GrillBot.Tests.App.Infrastructure.TypeReaders
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

            var reader = new BooleanTypeReader();
            foreach (var @case in cases)
            {
                var result = reader.ReadAsync(null, @case.Key, null).Result;

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

            var reader = new BooleanTypeReader();
            foreach (var @case in cases)
            {
                var result = reader.ReadAsync(null, @case.Key, null).Result;

                Assert.IsTrue(result.IsSuccess);
                Assert.AreEqual(1, result.Values.Count);
                Assert.AreEqual(@case.Value, result.Values.First().Value);
            }
        }

        [TestMethod]
        public void Read_Invalid()
        {
            var reader = new BooleanTypeReader();
            var result = reader.ReadAsync(null, "nikdy", null).Result;

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(CommandError.ParseFailed, result.Error);
        }
    }
}
