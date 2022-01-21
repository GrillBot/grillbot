using GrillBot.Data.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace GrillBot.Tests.Data.Extensions;

class Data
{
    public Data[] SubData { get; set; }
    public int Val { get; set; }
}

[TestClass]
public class EnumerableExtensionsTests
{
    [TestMethod]
    public void Flatten()
    {
        var data = new[] {
            new Data() { SubData = new[] { new Data() { Val = 3 } }, Val = 1 },
            new Data() { SubData = new[] { new Data() { Val = 4 } }, Val = 2 }
        };

        var expected = new[] { 1, 2, 3, 4 };

        var flattened = data.Flatten(o => o.SubData).ToList();
        var result = flattened.Select(o => o.Val).OrderBy(o => o).ToList();
        Assert.IsTrue(expected.SequenceEqual(result));
    }
}
