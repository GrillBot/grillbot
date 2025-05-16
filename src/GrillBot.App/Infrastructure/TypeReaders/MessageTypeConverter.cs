using Discord.Interactions;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Infrastructure.TypeReaders;

public class MessageTypeConverter : TypeConverter<IMessage>
{
    public override ApplicationCommandOptionType GetDiscordType()
        => ApplicationCommandOptionType.String;

    public override async Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
    {
        var value = (string)option.Value;
        var messageCache = services.GetRequiredService<IMessageCacheManager>();
        var locale = context.Interaction.UserLocale;

        if (ulong.TryParse(value, out var messageId))
        {
            var message = await messageCache.GetAsync(messageId, context.Channel);
            if (message is not null)
                return TypeReaderHelper.FromSuccess(message);
        }

        if (!Uri.IsWellFormedUriString(value, UriKind.Absolute))
            return TypeReaderHelper.ParseFailed(services, "Message/InvalidUri", locale);

        var uriMatch = Core.Helpers.MessageHelper.DiscordMessageUriRegex().Match(value);
        if (!uriMatch.Success)
            return TypeReaderHelper.ParseFailed(services, "Message/InvalidDiscordUriFormat", locale);
        if (uriMatch.Groups[1].Value == "@me")
            return TypeReaderHelper.Unsuccessful(services, "Message/DmUnsupported", locale); // DMs

        if (!ulong.TryParse(uriMatch.Groups[1].Value, out var guildId))
            return TypeReaderHelper.ParseFailed(services, "Message/InvalidGuildIdentifier", locale);

        var guild = await context.Client.GetGuildAsync(guildId);
        if (guild is null)
            return TypeReaderHelper.ConvertFailed(services, "Message/GuildNotFound", locale);

        if (!ulong.TryParse(uriMatch.Groups[2].Value, out var channelId))
            return TypeReaderHelper.ParseFailed(services, "Message/ChannelIdInvalidFormat", locale);

        var channel = await guild.GetTextChannelAsync(channelId);
        if (channel is null)
            return TypeReaderHelper.ConvertFailed(services, "Message/ChannelNotFound", locale);

        if (!ulong.TryParse(uriMatch.Groups[3].Value, out var msgId))
            return TypeReaderHelper.ParseFailed(services, "Message/InvalidMessageIdFormat", locale);

        var msg = await messageCache.GetAsync(msgId, channel);
        return msg is null ?
            TypeReaderHelper.ConvertFailed(services, "Message/UnknownMessage", locale) :
            TypeReaderHelper.FromSuccess(msg);
    }
}
