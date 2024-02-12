using Discord.Net;
using GrillBot.App.Managers;
using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Unverify;

namespace GrillBot.App.Actions.Api.V1.Unverify;

public class UpdateUnverify : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private ITextsManager Texts { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private UnverifyLogManager UnverifyLogManager { get; }
    private UnverifyMessageManager MessageManager { get; }
    private UnverifyRabbitMQManager UnverifyRabbitMQ { get; }

    public UpdateUnverify(ApiRequestContext apiContext, IDiscordClient discordClient, ITextsManager texts, GrillBotDatabaseBuilder databaseBuilder, UnverifyLogManager unverifyLogManager,
        UnverifyMessageManager messageManager, UnverifyRabbitMQManager unverifyRabbitMQ) : base(apiContext)
    {
        DiscordClient = discordClient;
        Texts = texts;
        DatabaseBuilder = databaseBuilder;
        UnverifyLogManager = unverifyLogManager;
        MessageManager = messageManager;
        UnverifyRabbitMQ = unverifyRabbitMQ;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var guildId = (ulong)Parameters[0]!;
        var userId = (ulong)Parameters[1]!;
        var parameters = (UpdateUnverifyParams)Parameters[2]!;

        var result = await ProcessAsync(guildId, userId, parameters);
        return ApiResult.Ok(new MessageResponse(result));
    }

    public async Task<string> ProcessAsync(ulong guildId, ulong userId, UpdateUnverifyParams parameters)
    {
        var (guild, fromUser, toUser) = await InitAsync(guildId, userId);

        await using var repository = DatabaseBuilder.CreateRepository();

        var user = await repository.GuildUser.FindGuildUserAsync(toUser, includeAll: true);
        EnsureValidUser(user, parameters.EndAt);
        await UnverifyLogManager.LogUpdateAsync(DateTime.Now, parameters.EndAt, guild, fromUser, toUser, parameters.Reason);
        await UnverifyRabbitMQ.SendModifyAsync(user!.Unverify!.SetOperationId, parameters.EndAt);

        user!.Unverify!.EndAt = parameters.EndAt;
        user.Unverify.StartAt = DateTime.Now;
        await repository.CommitAsync();

        await SendNotificationAsync(toUser, parameters.EndAt, parameters.Reason, user);
        return MessageManager.CreateUpdateChannelMessage(toUser, parameters.EndAt, parameters.Reason, ApiContext.Language);
    }

    private async Task<(IGuild guild, IGuildUser fromUser, IGuildUser toUser)> InitAsync(ulong guildId, ulong userId)
    {
        var guild = await DiscordClient.GetGuildAsync(guildId);
        var toUser = guild == null ? null : await guild.GetUserAsync(userId);
        var fromUser = guild == null ? null : await guild.GetUserAsync(ApiContext.GetUserId());

        if (guild == null)
            throw new NotFoundException(Texts["Unverify/GuildNotFound", ApiContext.Language]);
        if (toUser == null)
            throw new NotFoundException(Texts["Unverify/DestUserNotFound", ApiContext.Language]);

        return (guild, fromUser!, toUser);
    }

    private void EnsureValidUser(Database.Entity.GuildUser? user, DateTime endAt)
    {
        if (user?.Unverify == null)
            throw new NotFoundException(Texts["Unverify/Update/UnverifyNotFound", ApiContext.Language]);
        if ((user.Unverify.EndAt - DateTime.Now).TotalSeconds <= 30.0)
            throw new ValidationException(Texts["Unverify/Update/NotEnoughTime", ApiContext.Language]).ToBadRequestValidation(endAt, nameof(endAt));
    }

    private async Task SendNotificationAsync(IGuildUser toUser, DateTime newEnd, string? reason, Database.Entity.GuildUser userEntity)
    {
        try
        {
            var locale = JsonConvert.DeserializeObject<Data.Models.Unverify.UnverifyLogSet>(userEntity.Unverify!.UnverifyLog!.Data)!.Language
                         ?? ApiContext.Language;

            var dmMessage = MessageManager.CreateUpdatePmMessage(toUser.Guild, newEnd, reason, locale);
            await toUser.SendMessageAsync(dmMessage);
        }
        catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
        {
            // User have disabled DMs.
        }
    }
}
