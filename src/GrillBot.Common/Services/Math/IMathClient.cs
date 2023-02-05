using GrillBot.Common.Services.Math.Models;

namespace GrillBot.Common.Services.Math;

public interface IMathClient
{
    Task<MathJsResult> SolveExpressionAsync(MathJsRequest request);
}
