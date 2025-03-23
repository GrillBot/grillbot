using GrillBot.Common.Services.Math.Models;
using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.Common.Attributes;
using Refit;

namespace GrillBot.Common.Services.Math;

[Service("MathJS")]
public interface IMathClient : IServiceClient
{
    [Post("/")]
    Task<MathJsResult> SolveExpressionAsync(MathJsRequest request, CancellationToken cancellationToken = default);
}
