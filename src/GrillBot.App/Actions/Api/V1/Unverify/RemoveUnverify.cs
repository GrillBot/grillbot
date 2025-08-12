using GrillBot.App.Managers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Managers.Logging;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.Common.Exceptions;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Data.Models.API;
using UnverifyService;

namespace GrillBot.App.Actions.Api.V1.Unverify;

public class RemoveUnverify : ApiAction
{
    private readonly IServiceClientExecutor<IUnverifyServiceClient> _unverifyClient;
    private readonly ITextsManager _texts;
    private readonly IDiscordClient _discordClient;

    private IDiscordClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private UnverifyMessageManager MessageManager { get; }
    private LoggingManager LoggingManager { get; }

    public RemoveUnverify(ApiRequestContext apiContext, IDiscordClient discordClient, ITextsManager texts, GrillBotDatabaseBuilder databaseBuilder, UnverifyMessageManager messageManager,
        LoggingManager loggingManager, IServiceClientExecutor<IUnverifyServiceClient> unverifyClient) : base(apiContext)
    {
        DiscordClient = discordClient;
        DatabaseBuilder = databaseBuilder;
        MessageManager = messageManager;
        LoggingManager = loggingManager;
        _unverifyClient = unverifyClient;
        _texts = texts;
        _discordClient = discordClient;
    }

    // Bot
    public async Task ProcessAutoRemoveAsync(ulong guildId, ulong userId)
    {
        try
        {
            UpdateContext("cs", DiscordClient.CurrentUser);
            await ProcessAsync(guildId, userId);
        }
        catch (NotFoundException)
        {
            // There is not reason why throw exception if removal process is from job.  
            await ForceRemoveAsync(guildId, userId);
        }
    }

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
            return MessageManager.CreateRemoveAccessUnverifyNotFound(toUser!, ApiContext.Language);
        }
        catch (ClientException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
        {
            return _texts["Unverify/Forbidden", ApiContext.Language];
        }
        catch (ClientException ex)
        {
            await LoggingManager.ErrorAsync(nameof(RemoveUnverify), "An error occurred when removing unverify.", ex);

            return MessageManager.CreateRemoveAccessManuallyFailed(toUser!, ex, "cs");
        }
    }

    private async Task ForceRemoveAsync(ulong guildId, ulong userId)
    {
        using var repository = DatabaseBuilder.CreateRepository();

        var unverify = await repository.Unverify.FindUnverifyAsync(guildId, userId);
        if (unverify != null)
        {
            repository.Remove(unverify);
            await repository.CommitAsync();
        }
    }
}
