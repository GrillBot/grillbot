using GrillBot.App.Services.Unverify;
using System;
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
        var service = new UnverifyChecker(null, configuration, null);

        service.ValidateUnverifyDate(DateTime.MinValue, null, false);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(ValidationException))]
    public void ValidateUnverifyData_NotMinimal_Selfunverify()
    {
        var configuration = ConfigurationHelper.CreateConfiguration(new()
        {
            { "Unverify:MinimalTimes:Selfunverify", (12 * 60).ToString() }
        });

        var service = new UnverifyChecker(null, configuration, null);

        var end = DateTime.Now.AddHours(2);
        service.ValidateUnverifyDate(end, null, true);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(ValidationException))]
    public void ValidateUnverifyData_NotMinimal_Unverify()
    {
        var configuration = ConfigurationHelper.CreateConfiguration(new()
        {
            { "Unverify:MinimalTimes:Unverify", (12 * 60).ToString() }
        });

        var service = new UnverifyChecker(null, configuration, null);

        var end = DateTime.Now.AddHours(2);
        service.ValidateUnverifyDate(end, null, false);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(ValidationException))]
    public void ValidateUnverifyData_UsersMinimal_Selfunverify()
    {
        var configuration = ConfigurationHelper.CreateConfiguration(new()
        {
            { "Unverify:MinimalTimes:Selfunverify", (12 * 60).ToString() }
        });

        var service = new UnverifyChecker(null, configuration, null);

        var end = DateTime.Now.AddHours(2);
        service.ValidateUnverifyDate(end, TimeSpan.FromDays(2), true);
    }
}
