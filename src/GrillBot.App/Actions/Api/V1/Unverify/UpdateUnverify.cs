using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.Common.Exceptions;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Unverify;
using UnverifyService;
using UnverifyService.Models.Request;

namespace GrillBot.App.Actions.Api.V1.Unverify;

public class UpdateUnverify(
    ApiRequestContext apiContext,
    ITextsManager texts,
    IServiceClientExecutor<IUnverifyServiceClient> unverifyClient
) : ApiAction(apiContext)
{
    // API
    public override async Task<ApiResult> ProcessAsync()
    {
        var guildId = (ulong)Parameters[0]!;
        var userId = (ulong)Parameters[1]!;
        var parameters = (UpdateUnverifyParams)Parameters[2]!;

        var result = await ProcessAsync(guildId, userId, parameters);
        return ApiResult.Ok(new MessageResponse(result));
    }

    // Command
    public async Task<string> ProcessAsync(ulong guildId, ulong userId, UpdateUnverifyParams parameters)
    {
        try
        {
            var request = new UpdateUnverifyRequest
            {
                GuildId = guildId.ToString(),
                NewEndAtUtc = parameters.EndAt.WithKind(DateTimeKind.Local).ToUniversalTime(),
                Reason = parameters.Reason,
                UserId = userId.ToString()
            };

            var response = (await unverifyClient.ExecuteRequestAsync(
                async (client, ctx) => await client.UpdateUnverifyAsync(request, ctx.AuthorizationToken, ctx.CancellationToken),
                CancellationToken
            ))!;

            return texts[response, ApiContext.Language];
        }
        catch (ClientNotFoundException)
        {
            throw new NotFoundException(texts["Unverify/Update/UnverifyNotFound", ApiContext.Language]);
        }
        catch (ClientBadRequestException ex)
        {
            var validationError = ex.ValidationErrors.Values.SelectMany(o => o).FirstOrDefault();
            if (!string.IsNullOrEmpty(validationError))
                throw new ValidationException(texts[validationError, ApiContext.Language]).ToBadRequestValidation(parameters.EndAt, nameof(parameters.EndAt));
            throw;
        }
        catch (ClientException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
        {
            return texts["Unverify/Forbidden", ApiContext.Language];
        }
    }
}
