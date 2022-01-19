using GrillBot.Data.Models.API.Help;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace GrillBot.Tests.Data.Models.API.Help;

[TestClass]
public class TextBasedCommandTests
{
    [TestMethod]
    public void EmptyConstructor()
    {
        TestHelpers.CheckDefaultPropertyValues(new TextBasedCommand());
    }

    [TestMethod]
    public void FilledConstructor()
    {
        var command = new TextBasedCommand()
        {
            Aliases = new List<string>(),
            Command = "cmd",
            CommandId = "$cmd",
            Description = "Desc",
            Guilds = new List<string>(),
            Parameters = new List<string>()
        };

        TestHelpers.CheckNonDefaultPropertyValues(command);
    }
}
