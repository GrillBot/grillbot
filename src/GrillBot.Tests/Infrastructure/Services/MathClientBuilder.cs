using GrillBot.Common.Services.Math;
using GrillBot.Common.Services.Math.Models;
using Moq;

namespace GrillBot.Tests.Infrastructure.Services;

public class MathClientBuilder : BuilderBase<IMathClient>
{
    public MathClientBuilder SetSolveExpressionAction(string expression, MathJsResult result)
    {
        Mock.Setup(o => o.SolveExpressionAsync(It.Is<MathJsRequest>(req => req.Expression == expression))).ReturnsAsync(result);
        return this;
    }
}
