using System.Net;
using System.Text.Json;
using GrillBot.Common.Services.Math.Models;
using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.Common.Extensions;

namespace GrillBot.Common.Services.Math;

public class MathClient : RestServiceBase, IMathClient
{
    public override string ServiceName => "MathJS";

    public MathClient(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public async Task<MathJsResult> SolveExpressionAsync(MathJsRequest request)
    {
        try
        {
            return (await ProcessRequestAsync<MathJsResult>(
                () => HttpMethod.Post.ToRequest("", request),
                TimeSpan.FromSeconds(10)
            ))!;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            var response = ex.Data["ResponseContent"] as string;
            if (string.IsNullOrEmpty(response) || !response.StartsWith('{'))
                throw;

            return JsonSerializer.Deserialize<MathJsResult>(response)!;
        }
        catch (TaskCanceledException)
        {
            return new MathJsResult { IsTimeout = true };
        }
    }
}
