using Discord.Commands;

namespace GrillBot.App.Infrastructure.Preconditions.TextBased;

public class TextCommandDeprecatedAttribute : PreconditionAttribute
{
    public const string Prefix = "Tento příkaz již není v textové formě podporován.";

    public string AlternativeCommand { get; set; }
    public string AdditionalMessage { get; set; }

    public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        var msgBuilder = new StringBuilder(Prefix);

        if (!string.IsNullOrEmpty(AlternativeCommand))
            msgBuilder.Append($" Příkaz byl nahrazen příkazem `{AlternativeCommand}`");

        if (!string.IsNullOrEmpty(AdditionalMessage))
            msgBuilder.AppendLine().AppendLine(AdditionalMessage);

        return Task.FromResult(PreconditionResult.FromError(msgBuilder.ToString()));
    }
}
