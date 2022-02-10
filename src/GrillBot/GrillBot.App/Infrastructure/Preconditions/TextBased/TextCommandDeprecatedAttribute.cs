using Discord.Commands;

namespace GrillBot.App.Infrastructure.Preconditions.TextBased;

public class TextCommandDeprecatedAttribute : PreconditionAttribute
{
    public string AlternativeCommand { get; set; }

    public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        var msgBuilder = new StringBuilder("Tento příkaz již není v textové formě podporován.");

        if (!string.IsNullOrEmpty(AlternativeCommand))
            msgBuilder.AppendFormat(" Příkaz byl nahrazen příkazem `{0}`", AlternativeCommand);

        return Task.FromResult(PreconditionResult.FromError(msgBuilder.ToString()));
    }
}
