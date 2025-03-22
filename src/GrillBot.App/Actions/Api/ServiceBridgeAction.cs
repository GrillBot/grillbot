using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.Common.Exceptions;
using GrillBot.Data.Models.API;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Actions.Api;

public class ServiceBridgeAction<TServiceClient> : ApiAction where TServiceClient : IServiceClient
{
    private TServiceClient Client { get; }

    public ServiceBridgeAction(ApiRequestContext apiContext, TServiceClient client) : base(apiContext)
    {
        Client = client;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var funcExecutor = Parameters.OfType<Func<TServiceClient, Task<object>>>().FirstOrDefault();
        var actionExecutor = Parameters.OfType<Func<TServiceClient, Task>>().FirstOrDefault();

        try
        {
            if (funcExecutor is not null)
                return ApiResult.Ok(await funcExecutor(Client));

            if (actionExecutor is not null)
            {
                await actionExecutor(Client);
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
}
