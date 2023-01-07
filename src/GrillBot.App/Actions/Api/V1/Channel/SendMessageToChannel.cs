using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Extensions;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Data.Exceptions;
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
        var guild = await DiscordClient.GetGuildAsync(guildId, CacheMode.CacheOnly);
        if (guild == null) throw new NotFoundException(Texts["ChannelModule/PostMessage/GuildNotFound", ApiContext.Language]);

        var channel = await guild.GetTextChannelAsync(channelId);
        if (channel == null) throw new NotFoundException(Texts["ChannelModule/PostMessage/ChannelNotFound", ApiContext.Language].FormatWith(guild.Name));

        return channel;
    }

    private static MessageReference CreateReference(string reference, ulong guildId, ulong channelId)
    {
        if (string.IsNullOrEmpty(reference))
            return null;

        if (ulong.TryParse(reference, out var messageId))
            return new MessageReference(messageId, channelId, guildId);

        if (!Uri.IsWellFormedUriString(reference, UriKind.Absolute))
            return null;

        var uriMatch = MessageHelper.DiscordMessageUriRegex.Match(reference);
        return uriMatch.Success ? new MessageReference(uriMatch.Groups[3].Value.ToUlong(), channelId, guildId) : null;
    }
}
