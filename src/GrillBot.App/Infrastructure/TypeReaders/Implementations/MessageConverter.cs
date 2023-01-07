using Discord.Commands;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Helpers;
using GrillBot.Data.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Infrastructure.TypeReaders.Implementations;

public class MessageConverter : ConverterBase<IMessage>
{
    public MessageConverter(IServiceProvider provider, ICommandContext context) : base(provider, context)
    {
    }

    public MessageConverter(IServiceProvider provider, IInteractionContext context) : base(provider, context)
    {
    }

    public override async Task<IMessage> ConvertAsync(string value)
    {
        var messageCache = ServiceProvider.GetRequiredService<IMessageCacheManager>();
        if (ulong.TryParse(value, out var messageId))
        {
            var message = await messageCache.GetAsync(messageId, Channel);
            if (message != null) return message;
        }

        if (!Uri.IsWellFormedUriString(value, UriKind.Absolute))
            throw new FormatException(GetLocalizedText("Message/InvalidUri"));

        var uriMatch = MessageHelper.DiscordMessageUriRegex.Match(value);
        if (!uriMatch.Success) throw new UriFormatException(GetLocalizedText("Message/InvalidDiscordUriFormat"));
        if (uriMatch.Groups[1].Value == "@me") throw new InvalidOperationException(GetLocalizedText("Message/DmUnsupported")); // Is DM

        if (!ulong.TryParse(uriMatch.Groups[1].Value, out var guildId)) throw new FormatException(GetLocalizedText("Message/InvalidGuildIdentifier"));
        var guild = await Client.GetGuildAsync(guildId);
        if (guild == null) throw new NotFoundException(GetLocalizedText("Message/GuildNotFound"));

        if (!ulong.TryParse(uriMatch.Groups[2].Value, out var channelId)) throw new FormatException(GetLocalizedText("Message/ChannelIdInvalidFormat"));
        var channel = await guild.GetTextChannelAsync(channelId);
        if (channel == null) throw new NotFoundException(GetLocalizedText("Message/ChannelNotFound"));

        if (!ulong.TryParse(uriMatch.Groups[3].Value, out var msgId)) throw new FormatException(GetLocalizedText("Message/InvalidMessageIdFormat"));
        var msg = await messageCache.GetAsync(msgId, channel);
        if (msg == null) throw new NotFoundException(GetLocalizedText("Message/UnknownMessage"));

        return msg;
    }
}
