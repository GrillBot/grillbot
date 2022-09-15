using GrillBot.App.Services.Unverify;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.App.Services.Unverify;

[TestClass]
public class UnverifyCheckerTests
{
    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(ValidationException))]
    public void ValidateUnverifyDate_Ends()
    {
        var configuration = ConfigurationHelper.CreateConfiguration();
        var texts = new TextsBuilder()
            .AddText("Unverify/Validation/MinimalTime", "cs", "{0}")
            .Build();
        var service = new UnverifyChecker(null, configuration, null, texts);

        service.ValidateUnverifyDate(DateTime.MinValue, null, false, "cs");
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(ValidationException))]
    public void ValidateUnverifyData_NotMinimal_Selfunverify()
    {
        var configuration = ConfigurationHelper.CreateConfiguration(new Dictionary<string, string>
        {
            { "Unverify:MinimalTimes:Selfunverify", (12 * 60).ToString() }
        });

        var texts = new TextsBuilder()
            .AddText("Unverify/Validation/MinimalTime", "cs", "{0}")
            .Build();
        var service = new UnverifyChecker(null, configuration, null, texts);

        var end = DateTime.Now.AddHours(2);
        service.ValidateUnverifyDate(end, null, true, "cs");
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(ValidationException))]
    public void ValidateUnverifyData_NotMinimal_Unverify()
    {
        var configuration = ConfigurationHelper.CreateConfiguration(new Dictionary<string, string>
        {
            { "Unverify:MinimalTimes:Unverify", (12 * 60).ToString() }
        });

        var texts = new TextsBuilder()
            .AddText("Unverify/Validation/MinimalTime", "cs", "{0}")
            .Build();
        var service = new UnverifyChecker(null, configuration, null, texts);

        var end = DateTime.Now.AddHours(2);
        service.ValidateUnverifyDate(end, null, false, "cs");
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(ValidationException))]
    public void ValidateUnverifyData_UsersMinimal_Selfunverify()
    {
        var configuration = ConfigurationHelper.CreateConfiguration(new Dictionary<string, string>
        {
            { "Unverify:MinimalTimes:Selfunverify", (12 * 60).ToString() }
        });

        var texts = new TextsBuilder()
            .AddText("Unverify/Validation/MinimalTime", "cs", "{0}")
            .Build();
        var service = new UnverifyChecker(null, configuration, null, texts);

        var end = DateTime.Now.AddHours(2);
        service.ValidateUnverifyDate(end, TimeSpan.FromDays(2), true, "cs");
    }
}
