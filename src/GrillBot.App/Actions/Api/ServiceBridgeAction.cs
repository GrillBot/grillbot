using GrillBot.Common.Models;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.Common.Exceptions;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Data.Models.API;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Actions.Api;

public class ServiceBridgeAction<TServiceClient>(
    ApiRequestContext apiContext,
    IServiceClientExecutor<TServiceClient> _client
) : ApiAction(apiContext) where TServiceClient : IServiceClient
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var funcExecutor = Parameters.OfType<Func<TServiceClient, ServiceExecutorContext, Task<object>>>().FirstOrDefault();
        var actionExecutor = Parameters.OfType<Func<TServiceClient, ServiceExecutorContext, Task>>().FirstOrDefault();

        try
        {
            if (funcExecutor is not null)
                return await ExecuteRequestAsync(funcExecutor);

            if (actionExecutor is not null)
            {
                await _client.ExecuteRequestAsync((c, ctx) => actionExecutor(c, ctx));
                return ApiResult.Ok();
            }
        }
        catch (ClientNotFoundException)
        {
            return ApiResult.NotFound();
        }
        catch (ClientNotAcceptableException)
        {
            return new ApiResult(StatusCodes.Status406NotAcceptable);
        }
        catch (ClientBadRequestException ex)
        {
            var problemDetails = new ValidationProblemDetails(ex.ValidationErrors);
            return ApiResult.BadRequest(problemDetails);
        }
        catch (ClientException ex)
        {
            var statusCode = ex.StatusCode ?? HttpStatusCode.InternalServerError;
            return new ApiResult((int)statusCode, new MessageResponse(ex.Message));
        }

        return ApiResult.BadRequest();
    }

    private async Task<ApiResult> ExecuteRequestAsync(Func<TServiceClient, ServiceExecutorContext, Task<object>> funcExecutor)
    {
        object? result = null;

        try
        {
            result = await _client.ExecuteRequestAsync((c, ctx) => funcExecutor(c, ctx));

            if (result is Stream stream)
            {
                var streamContent = await stream.ToByteArrayAsync();
                return ApiResult.Ok(new FileContentResult(streamContent, "application/octet-stream"));
            }

            return ApiResult.Ok(result);
        }
        finally
        {
            (result as IDisposable)?.Dispose();
        }
    }
}
