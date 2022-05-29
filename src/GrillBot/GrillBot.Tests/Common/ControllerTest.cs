﻿using GrillBot.Cache.Services;
using GrillBot.Cache.Services.Repository;
using GrillBot.Data.Models.API;
using GrillBot.Database.Services;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Cache;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace GrillBot.Tests.Common;

[ExcludeFromCodeCoverage]
public abstract class ControllerTest<TController> where TController : Controller
{
    protected TController AdminController { get; private set; }
    protected TController UserController { get; private set; }

    protected GrillBotContext DbContext { get; set; }
    protected GrillBotDatabaseFactory DbFactory { get; set; }
    protected GrillBotCacheRepository CacheRepository { get; set; }
    protected GrillBotCacheBuilder CacheBuilder { get; set; }

    protected abstract bool CanInitProvider();
    protected abstract TController CreateController(IServiceProvider provider);

    [TestInitialize]
    public void Initialize()
    {
        DbFactory = new DbContextBuilder();
        DbContext = DbFactory.Create();

        CacheBuilder = new TestCacheBuilder();
        CacheRepository = CacheBuilder.CreateRepository();

        var provider = CreateProvider(CanInitProvider());

        AdminController = CreateController(provider);
        AdminController.ControllerContext = CreateContext("Admin", provider);

        UserController = CreateController(provider);
        UserController.ControllerContext = CreateContext("User", provider);
    }

    public virtual void Cleanup() { }

    [TestCleanup]
    public void TestClean()
    {
        DbContext.ChangeTracker.Clear();
        DatabaseHelper.ClearDatabase(DbContext);
        TestCacheBuilder.ClearDatabase(CacheRepository);

        Cleanup();

        DbContext.Dispose();
        AdminController.Dispose();
        UserController.Dispose();
        CacheRepository.Dispose();
    }

    private static ControllerContext CreateContext(string role, IServiceProvider provider)
    {
        return new ControllerContext()
        {
            HttpContext = new DefaultHttpContext()
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Role, role),
                    new Claim(ClaimTypes.NameIdentifier, Consts.UserId.ToString())
                })),
                RequestServices = provider
            }
        };
    }

    protected void CheckResult<TResult>(IActionResult result) where TResult : IActionResult
    {
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(TResult));

        if (result is NotFoundObjectResult notFound)
        {
            Assert.IsInstanceOfType(notFound.Value, typeof(MessageResponse));
        }
        else if (result is FileContentResult fileContent)
        {
            Assert.IsNotNull(fileContent.FileContents);
            Assert.IsTrue(fileContent.FileContents.Length > 0);
        }
    }

    protected void CheckResult<TResult, TOkModel>(ActionResult<TOkModel> result) where TResult : ObjectResult
    {
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(TResult));

        if (result.Result is OkObjectResult ok)
            Assert.IsInstanceOfType(ok.Value, typeof(TOkModel));
        else if (result.Result is NotFoundObjectResult notFound)
            Assert.IsInstanceOfType(notFound.Value, typeof(MessageResponse));
        else if (result.Result is BadRequestObjectResult badRequest)
            Assert.IsInstanceOfType(badRequest.Value, typeof(ValidationProblemDetails));
    }

    protected IServiceProvider CreateProvider(bool init = false)
    {
        return init ?
            DIHelper.CreateInitializedProvider() :
            DIHelper.CreateEmptyProvider();
    }
}
