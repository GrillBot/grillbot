using GrillBot.App.Managers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Managers.Logging;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.Common.Exceptions;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Data.Models.API;
using UnverifyService;

namespace GrillBot.App.Actions.Api.V1.Unverify;

public class RemoveUnverify(
    ApiRequestContext apiContext,
    IDiscordClient _discordClient,
    ITextsManager _texts,
    UnverifyMessageManager _messageManager,
    LoggingManager _loggingManager,
    IServiceClientExecutor<IUnverifyServiceClient> _unverifyClient
) : ApiAction(apiContext)
{

    // API
    public override async Task<ApiResult> ProcessAsync()
    {
        var guildId = (ulong)Parameters[0]!;
        var userId = (ulong)Parameters[1]!;
        var force = (bool)Parameters[2]!;

        var result = await ProcessAsync(guildId, userId, force);
        return ApiResult.Ok(new MessageResponse(result));
    }

    // Command
    public async Task<string> ProcessAsync(ulong guildId, ulong userId, bool force = false)
    {
        var toUser = await _discordClient.FindGuildUserAsync(guildId, userId, CancellationToken);

        try
        {
            var response = await _unverifyClient.ExecuteRequestAsync(
                async (client, ctx) => await client.RemoveUnverifyAsync(guildId, userId, force, ctx.AuthorizationToken, ctx.CancellationToken),
                CancellationToken
            );

            return _texts[response!.Message, ApiContext.Language];
        }
        catch (ClientNotFoundException)
        {
            return _messageManager.CreateRemoveAccessUnverifyNotFound(toUser!, ApiContext.Language);
        }
        catch (ClientException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
        {
            return _texts["Unverify/Forbidden", ApiContext.Language];
        }
        catch (ClientException ex)
        {
            await _loggingManager.ErrorAsync(nameof(RemoveUnverify), "An error occurred when removing unverify.", ex);
            return _messageManager.CreateRemoveAccessManuallyFailed(toUser!, ex, "cs");
        }
    }
}
