using GrillBot.Data.Models.API.Help;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.API.Help;

[TestClass]
public class CommandGroupTests
{
    [TestMethod]
    public void EmptyConstructor()
    {
        TestHelpers.CheckDefaultPropertyValues(new CommandGroup(), (defaultVal, val, name) =>
            {
                if (name != "Commands")
                    Assert.AreEqual(defaultVal, val);
                else
                    Assert.AreNotEqual(defaultVal, val);
            });
    }

    [TestMethod]
    public void FilledConstructor()
    {
        var group = new CommandGroup()
        {
            Commands = new(),
            Description = "Desc",
            GroupName = "Grp"
        };
        TestHelpers.CheckNonDefaultPropertyValues(group);
    }
}
