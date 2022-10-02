using GrillBot.App.Services.User.Points;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Data.Exceptions;

namespace GrillBot.App.Actions.Api.V1.Points;

public class ServiceTransferPoints : ApiAction
{
    private PointsService PointsService { get; }
    private IDiscordClient DiscordClient { get; }
    private ITextsManager Texts { get; }

    public ServiceTransferPoints(ApiRequestContext apiContext, PointsService pointsService, IDiscordClient discordClient, ITextsManager texts) : base(apiContext)
    {
        PointsService = pointsService;
        DiscordClient = discordClient;
        Texts = texts;
    }

    public async Task ProcessAsync(ulong guildId, ulong fromUserId, ulong toUserId, int amount)
    {
        var (from, to) = await GetAndCheckUsersAsync(guildId, fromUserId, toUserId);
        await PointsService.TransferPointsAsync(from, to, amount, ApiContext.Language);
    }

    private async Task<(IGuildUser from, IGuildUser to)> GetAndCheckUsersAsync(ulong guildId, ulong fromUserId, ulong toUserId)
    {
        if (fromUserId == toUserId)
            throw new ValidationException(Texts["Points/Service/Transfer/SameAccounts", ApiContext.Language]).ToBadRequestValidation($"{fromUserId}->{toUserId}", nameof(fromUserId), nameof(toUserId));

        var guild = await DiscordClient.GetGuildAsync(guildId);
        if (guild == null) throw new NotFoundException(Texts["Points/Service/Transfer/GuildNotFound", ApiContext.Language]);

        var fromUser = await guild.GetUserAsync(fromUserId);
        var toUser = await guild.GetUserAsync(toUserId);

        return (CheckUser(fromUser, true), CheckUser(toUser, false));
    }

    private IGuildUser CheckUser(IGuildUser user, bool isSource)
    {
        if (user == null)
            throw new NotFoundException(Texts[$"Points/Service/Transfer/{(isSource ? "SourceUserNotFound" : "DestUserNotFound")}", ApiContext.Language]);
        if (!user.IsUser())
            throw new ValidationException(Texts["Points/Service/Transfer/UserIsBot", ApiContext.Language].FormatWith(user.GetFullName())).ToBadRequestValidation(user.Id,
                isSource ? "fromUserId" : "toUserId");
        return user;
    }
}
