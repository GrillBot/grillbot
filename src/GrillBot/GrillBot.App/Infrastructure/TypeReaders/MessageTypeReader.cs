using Discord;
using Discord.Commands;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrillBot.App.Infrastructure.TypeReaders
{
    public class MessageTypeReader : TypeReader
    {
        public static Regex DiscordUriRegex { get; } = new Regex(@"https:\/\/discord\.com\/channels\/(@me|\d*)\/(\d+)\/(\d+)");

        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var ogTypeReader = new MessageTypeReader<IMessage>();
            var message = await ogTypeReader.ReadAsync(context, input, services);

            if (message.IsSuccess)
                return message;

            if (!Uri.IsWellFormedUriString(input, UriKind.Absolute))
                return TypeReaderResult.FromError(CommandError.ParseFailed, "Zadaná zpráva není ani identifikátor, ani odkaz.");

            var uriMatch = DiscordUriRegex.Match(input);
            if (!uriMatch.Success)
                return TypeReaderResult.FromError(CommandError.ParseFailed, "Zadaný odkaz není ve správném formátu odkazující na Discord zprávu.");

            if (uriMatch.Groups[1].Value == "@me")
            {
                // Is DM
                return TypeReaderResult.FromError(CommandError.Unsuccessful, "Použití odkazů na soukromou konverzaci není podporován. Pokud chceš použít soukromou konverzaci, " +
                    "pak zavolej příkaz v soukromé konverzaci s identifikátorem zprávy.");
            }
            else
            {
                if (!ulong.TryParse(uriMatch.Groups[1].Value, out ulong guildId))
                    return TypeReaderResult.FromError(CommandError.ParseFailed, "Nesprávný formát identifikátoru serveru.");

                var guild = await context.Client.GetGuildAsync(guildId);
                if (guild == null)
                    return TypeReaderResult.FromError(CommandError.ObjectNotFound, "Identifikátor serveru v odkazu odkazuje na server, kde se bot nenachází.");

                if (!ulong.TryParse(uriMatch.Groups[2].Value, out ulong channelId))
                    return TypeReaderResult.FromError(CommandError.ParseFailed, "Nesprávný formát identifikátoru kanálu.");

                var channel = await guild.GetTextChannelAsync(channelId);
                if (channel == null)
                    return TypeReaderResult.FromError(CommandError.ObjectNotFound, "Identifikátor kanálu v odkazu odkazuje na neexistující kanál.");

                if (!ulong.TryParse(uriMatch.Groups[3].Value, out ulong messageId))
                    return TypeReaderResult.FromError(CommandError.ParseFailed, "Nesprávný formát identifikátoru zprávy.");

                var msg = await channel.GetMessageAsync(messageId);
                if (msg == null)
                    return TypeReaderResult.FromError(CommandError.ObjectNotFound, "Identifikátor zprávy v odkazu odkazuje na neexistující zprávu.");

                return TypeReaderResult.FromSuccess(msg);
            }
        }
    }
}
