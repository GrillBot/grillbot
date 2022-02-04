using GrillBot.App.Services.Unverify;
using GrillBot.Tests.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Services.Unverify;

[TestClass]
public class SelfunverifyServiceTests : ServiceTest<SelfunverifyService>
{
    protected override SelfunverifyService CreateService()
    {
        var dbFactory = new DbContextBuilder();
        DbContext = dbFactory.Create();

        return new SelfunverifyService(null, dbFactory);
    }

    public override void Cleanup()
    {
        DbContext.RemoveRange(DbContext.SelfunverifyKeepables.AsEnumerable());
        DbContext.SaveChangesAsync();
    }

    private async Task FillDataAsync()
    {
        await DbContext.SelfunverifyKeepables.AddAsync(new Database.Entity.SelfunverifyKeepable() { GroupName = "a", Name = "b" });
        await DbContext.SaveChangesAsync();
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(ValidationException))]
    public async Task AddKeepableAsync_Exists()
    {
        await FillDataAsync();
        await Service.AddKeepableAsync("A", "B");
    }

    [TestMethod]
    public async Task AddKeepables_NotExists()
    {
        await Service.AddKeepableAsync("A", "B");
        Assert.IsTrue(true);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(ValidationException))]
    public async Task RemoveKeepableAsync_Group_NotExists()
    {
        await Service.RemoveKeepableAsync("a");
    }

    [TestMethod]
    public async Task RemoveKeepableAsync_Group_Success()
    {
        await FillDataAsync();
        await Service.RemoveKeepableAsync("a");
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task RemoveKeepableAsync_Item_Success()
    {
        await FillDataAsync();
        await Service.RemoveKeepableAsync("a", "b");
        Assert.IsTrue(true);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(ValidationException))]
    public async Task RemoveKeepableAsync_Item_NotExists()
    {
        await Service.RemoveKeepableAsync("a", "b");
    }

    [TestMethod]
    public async Task GetKeepablesAsync_WithoutFilter()
    {
        await FillDataAsync();
        var result = await Service.GetKeepablesAsync(null);
        Assert.AreEqual(1, result.Count);
    }
}
