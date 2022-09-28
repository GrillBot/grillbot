using GrillBot.App.Services.Unverify;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API.Unverify;

namespace GrillBot.App.Actions.Api.V1.Unverify;

public class UpdateUnverify : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private UnverifyService UnverifyService { get; }
    private ITextsManager Texts { get; }

    public UpdateUnverify(ApiRequestContext apiContext, IDiscordClient discordClient, UnverifyService unverifyService, ITextsManager texts) : base(apiContext)
    {
        DiscordClient = discordClient;
        UnverifyService = unverifyService;
        Texts = texts;
    }

    public async Task<string> ProcessAsync(ulong guildId, ulong userId, UpdateUnverifyParams parameters)
    {
        var (guild, fromUser, toUser) = await InitAsync(guildId, userId);

        return await UnverifyService.UpdateUnverifyAsync(toUser, guild, parameters.EndAt, fromUser, ApiContext.Language);
    }

    private async Task<(IGuild guild, IGuildUser fromUser, IGuildUser toUser)> InitAsync(ulong guildId, ulong userId)
    {
        var guild = await DiscordClient.GetGuildAsync(guildId);
        var toUser = guild == null ? null : await guild.GetUserAsync(userId);
        var fromUser = guild == null ? null : await guild.GetUserAsync(ApiContext.GetUserId());

        ValidateData(guild, toUser);
        return (guild, fromUser, toUser);
    }

    private void ValidateData(IGuild guild, IGuildUser toUser)
    {
        if (guild == null)
            throw new NotFoundException(Texts["Unverify/GuildNotFound", ApiContext.Language]);
        if (toUser == null)
            throw new NotFoundException(Texts["Unverify/DestUserNotFound", ApiContext.Language]);
    }
}
