using Discord;
using Discord.Commands;
using GrillBot.App.Services.MessageCache;
using GrillBot.Data.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrillBot.App.Infrastructure.TypeReaders.Implementations;

public class MessageConverter : ConverterBase<IMessage>
{
    public static Regex DiscordUriRegex { get; } = new Regex(@"https:\/\/discord\.com\/channels\/(@me|\d*)\/(\d+)\/(\d+)");

    public MessageConverter(IServiceProvider provider, ICommandContext context) : base(provider, context)
    {
    }

    public override async Task<IMessage> ConvertAsync(string value)
    {
        if (ulong.TryParse(value, out var messageId))
        {
            var messageCache = ServiceProvider.GetRequiredService<MessageCache>();
            var message = await messageCache.GetMessageAsync(Channel, messageId);

            if (message != null)
                return message;
        }

        if (!Uri.IsWellFormedUriString(value, UriKind.Absolute))
            throw new FormatException("Zadaná zpráva není ani identifikátor, ani odkaz.");

        var uriMatch = DiscordUriRegex.Match(value);
        if (!uriMatch.Success)
            throw new UriFormatException("Zadaný odkaz není ve správném formátu odkazující na Discord zprávu.");

        if (uriMatch.Groups[1].Value == "@me")
        {
            // Is DM
            throw new InvalidOperationException("Použití odkazů na soukromou konverzaci není podporován. Pokud chceš použít soukromou konverzaci, " +
                "pak zavolej příkaz v soukromé konverzaci s identifikátorem zprávy.");
        }
        else
        {
            if (!ulong.TryParse(uriMatch.Groups[1].Value, out ulong guildId))
                throw new FormatException("Nesprávný formát identifikátoru serveru.");

            var guild = await Client.GetGuildAsync(guildId);
            if (guild == null)
                throw new NotFoundException("Identifikátor serveru v odkazu odkazuje na server, kde se bot nenachází.");

            if (!ulong.TryParse(uriMatch.Groups[2].Value, out ulong channelId))
                throw new FormatException("Nesprávný formát identifikátoru kanálu.");

            var channel = await guild.GetTextChannelAsync(channelId);
            if (channel == null)
                throw new NotFoundException("Identifikátor kanálu v odkazu odkazuje na neexistující kanál.");

            if (!ulong.TryParse(uriMatch.Groups[3].Value, out ulong msgId))
                throw new FormatException("Nesprávný formát identifikátoru zprávy.");

            var msg = await channel.GetMessageAsync(msgId);
            if (msg == null)
                throw new NotFoundException("Identifikátor zprávy v odkazu odkazuje na neexistující zprávu.");

            return msg;
        }
    }
}
