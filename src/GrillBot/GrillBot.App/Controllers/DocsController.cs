using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System.Reflection;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/docs")]
[OpenApiTag("Docs", Description = "Documentation")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class DocsController : Controller
{
    /// <summary>
    /// Get dot definition for GraphViz of namespaces used in project.
    /// </summary>
    [HttpGet("namespaces")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces("text/plain")]
    public ActionResult<string> GetNamespaceGraph()
    {
        var assembly = Assembly.GetExecutingAssembly();

        var namespaces = assembly.GetReferencedAssemblies()
            .Where(o => o.Name.StartsWith("GrillBot"))
            .Select(o => Assembly.Load(o))
            .SelectMany(o => o.GetTypes())
            .Concat(assembly.GetTypes())
            .Select(o => o.Namespace)
            .Where(o => !string.IsNullOrEmpty(o) && o.StartsWith("GrillBot."))
            .Distinct()
            .Select(o => string.Join(" -> ", o.Split(".").Select(x => $"\"{x}\"")))
            .OrderBy(o => o)
            .ToList();

        var builder = new StringBuilder()
            .AppendLine("digraph G {")
            .AppendLine("rankdir=LR")
            .AppendLine("graph [concentrate=true];")
            .AppendLine("node[shape = box,]");

        foreach (var ns in namespaces)
            builder.AppendLine(ns);

        builder.AppendLine("}");
        return Ok(builder.ToString());
    }
}
