using Discord.Commands;
using GrillBot.Common.Extensions.Discord;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Infrastructure.Preconditions.TextBased;

public class TextCommandDeprecatedAttribute : PreconditionAttribute
{
    public const string Prefix = "Tento příkaz již není v textové formě podporován.";

    public string AlternativeCommand { get; set; }
    public string AdditionalMessage { get; set; }

    public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        var msgBuilder = new StringBuilder(Prefix);

        if (!string.IsNullOrEmpty(AlternativeCommand))
        {
            var interactionService = services.GetRequiredService<Discord.Interactions.InteractionService>();
            var commands = await interactionService.RestClient.GetGuildApplicationCommands(context.Guild.Id);
            var mentions = commands.GetCommandMentions();
            var commandText = mentions.TryGetValue(AlternativeCommand[1..], out var mention) ? mention : $"`{AlternativeCommand}`";

            msgBuilder.Append($" Příkaz byl nahrazen příkazem {commandText}");
        }

        if (!string.IsNullOrEmpty(AdditionalMessage))
            msgBuilder.AppendLine().AppendLine(AdditionalMessage);

        return PreconditionResult.FromError(msgBuilder.ToString());
    }
}
