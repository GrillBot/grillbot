using GrillBot.App.Services.Unverify;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Data.Exceptions;

namespace GrillBot.App.Actions.Api.V1.Unverify;

public class RemoveUnverify : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private UnverifyService UnverifyService { get; }
    private ITextsManager Texts { get; }

    public RemoveUnverify(ApiRequestContext apiContext, IDiscordClient discordClient, UnverifyService unverifyService, ITextsManager texts) : base(apiContext)
    {
        DiscordClient = discordClient;
        UnverifyService = unverifyService;
        Texts = texts;
    }

    public async Task<string> ProcessAsync(ulong guildId, ulong userId)
    {
        var (guild, toUser, fromUser) = await InitAsync(guildId, userId);

        return await UnverifyService.RemoveUnverifyAsync(guild, fromUser, toUser, ApiContext.Language, false, true);
    }

    private async Task<(IGuild guild, IGuildUser toUser, IGuildUser fromUser)> InitAsync(ulong guildId, ulong userId)
    {
        var guild = await DiscordClient.GetGuildAsync(guildId);
        var toUser = guild == null ? null : await guild.GetUserAsync(userId);
        var fromUser = guild == null ? null : await guild.GetUserAsync(ApiContext.GetUserId());

        ValidateData(guild, toUser);
        return (guild, toUser, fromUser);
    }

    private void ValidateData(IGuild guild, IGuildUser toUser)
    {
        if (guild == null)
            throw new NotFoundException(Texts["Unverify/GuildNotFound", ApiContext.Language]);
        if (toUser == null)
            throw new NotFoundException(Texts["Unverify/DestUserNotFound", ApiContext.Language]);
    }
}
