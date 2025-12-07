using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Models.API.Channels;

namespace GrillBot.App.Actions.Api.V1.Channel;

public class SendMessageToChannel : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private ITextsManager Texts { get; }
    private IMessageCacheManager MessageCache { get; }

    public SendMessageToChannel(ApiRequestContext apiContext, ITextsManager texts, IDiscordClient discordClient, IMessageCacheManager messageCache) : base(apiContext)
    {
        Texts = texts;
        DiscordClient = discordClient;
        MessageCache = messageCache;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var guildId = (ulong)Parameters[0]!;
        var channelId = (ulong)Parameters[1]!;
        var parameters = (SendMessageToChannelParams)Parameters[2]!;

        await ProcessAsync(guildId, channelId, parameters);
        return ApiResult.Ok();
    }

    public async Task ProcessAsync(ulong guildId, ulong channelId, SendMessageToChannelParams parameters)
    {
        var channel = await FindChannelAsync(guildId, channelId);

        var reference = CreateReference(parameters.Reference, guildId, channelId);
        if (reference != null && await MessageCache.GetAsync(reference.MessageId.Value, channel) == null)
            reference = null;

        if (parameters.Attachments.Count > 0)
            await channel.SendFilesAsync(parameters.Attachments, parameters.Content, messageReference: reference);
        else
            await channel.SendMessageAsync(parameters.Content, messageReference: reference);
    }

    private async Task<ITextChannel> FindChannelAsync(ulong guildId, ulong channelId)
    {
        var guild = await DiscordClient.GetGuildAsync(guildId, CacheMode.CacheOnly)
            ?? throw new NotFoundException(Texts["ChannelModule/PostMessage/GuildNotFound", ApiContext.Language]);

        var channel = await guild.GetTextChannelAsync(channelId);
        return (ITextChannel?)channel ?? throw new NotFoundException(string.Format(Texts["ChannelModule/PostMessage/ChannelNotFound", ApiContext.Language], guild.Name));
    }

    private static MessageReference? CreateReference(string? reference, ulong guildId, ulong channelId)
    {
        if (string.IsNullOrEmpty(reference))
            return null;

        if (ulong.TryParse(reference, out var messageId))
            return new MessageReference(messageId, channelId, guildId);

        if (!Uri.IsWellFormedUriString(reference, UriKind.Absolute))
            return null;

        var uriMatch = Core.Helpers.MessageHelper.DiscordMessageUriRegex().Match(reference);
        return uriMatch.Success ? new MessageReference(uriMatch.Groups[3].Value.ToUlong(), channelId, guildId) : null;
    }
}
