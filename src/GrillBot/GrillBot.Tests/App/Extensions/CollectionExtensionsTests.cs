using GrillBot.App.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Extensions
{
    [TestClass]
    public class CollectionExtensionsTests
    {
        [TestMethod]
        public void FindAll_A_sync()
        {
            var data = new List<int>() { 1, 2, 3, 4, 5 };
            var odds = data.FindAllAsync(async val => await Task.Run(() => (val % 2) == 0)).Result;

            Assert.AreEqual(2, odds.Count);
            Assert.IsTrue(odds.SequenceEqual(new[] { 2, 4 }));
        }

        [TestMethod]
        public void SplitInParts()
        {
            var data = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var chunks = data.SplitInParts(5).ToList();

            Assert.AreEqual(2, chunks.Count);
            Assert.IsTrue(new[] { 0, 1, 2, 3, 4 }.SequenceEqual(chunks[0]));
            Assert.IsTrue(new[] { 5, 6, 7, 8, 9 }.SequenceEqual(chunks[1]));
        }
    }
}
