using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.App.Managers;

[TestClass]
public class UnverifyCheckManager
{
    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(ValidationException))]
    public void ValidateUnverifyDate_Ends()
    {
        var configuration = ConfigurationHelper.CreateConfiguration();
        var service = new GrillBot.App.Managers.UnverifyCheckManager(null!, configuration, null!, TestServices.Texts.Value);

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

        var service = new GrillBot.App.Managers.UnverifyCheckManager(null!, configuration, null!, TestServices.Texts.Value);
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

        var service = new GrillBot.App.Managers.UnverifyCheckManager(null!, configuration, null!, TestServices.Texts.Value);
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

        var service = new GrillBot.App.Managers.UnverifyCheckManager(null!, configuration, null!, TestServices.Texts.Value);
        var end = DateTime.Now.AddHours(2);
        service.ValidateUnverifyDate(end, TimeSpan.FromDays(2), true, "cs");
    }
}
