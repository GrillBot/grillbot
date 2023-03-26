using Microsoft.AspNetCore.Mvc;

namespace GrillBot.Common.Services.Common;

public class RestResponse<TResponse>
{
    public TResponse? Response { get; set; }
    public ValidationProblemDetails? ValidationErrors { get; set; }

    public RestResponse(TResponse? response)
    {
        Response = response;
    }

    public RestResponse(ValidationProblemDetails validationErrors)
    {
        ValidationErrors = validationErrors;
    }
}
