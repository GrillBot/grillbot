using Discord.Commands;

namespace GrillBot.App.Infrastructure.Preconditions
{
    public class TextCommandDeprecatedAttribute : PreconditionAttribute
    {
        public string AlternativeCommand { get; set; }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var message = "Tento příkaz již není v textové formě podporován." + (!string.IsNullOrEmpty(AlternativeCommand) ? $" Příkaz byl nahrazen příkazem `{AlternativeCommand}`" : "");
            return Task.FromResult(PreconditionResult.FromError(message));
        }
    }
}
