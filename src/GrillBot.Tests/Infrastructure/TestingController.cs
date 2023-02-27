using Microsoft.AspNetCore.Mvc;

namespace GrillBot.Tests.Infrastructure;

public class TestingController : Controller
{
    public ActionResult TestMethod() => Ok();
}
