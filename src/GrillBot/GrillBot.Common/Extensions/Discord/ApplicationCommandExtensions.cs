using Discord;
using Discord.Rest;

namespace GrillBot.Common.Extensions.Discord;

public static class ApplicationCommandExtensions
{
    public static Dictionary<string, string> GetCommandMentions(this IEnumerable<RestGuildCommand> commands)
    {
        return commands.Where(o => o.Type == ApplicationCommandType.Slash)
            .SelectMany(ProcessCommand)
            .ToDictionary(cmd => cmd.Key, cmd => cmd.Value);
    }

    private static Dictionary<string, string> ProcessCommand(RestApplicationCommand command)
    {
        var result = new Dictionary<string, string>();

        var subCommands = command.Options.Where(o => o.Type is ApplicationCommandOptionType.SubCommand or ApplicationCommandOptionType.SubCommandGroup).ToList();
        if (subCommands.Count == 0)
        {
            result.Add(command.Name, $"</{command.Name}:{command.Id}>");
            return result;
        }

        foreach (var option in subCommands)
            ProcessOption(option, command.Name, command.Id, result);

        return result;
    }

    private static void ProcessOption(IApplicationCommandOption option, string prefix, ulong commandId, Dictionary<string, string> result)
    {
        if (option.Type == ApplicationCommandOptionType.SubCommand)
        {
            result.Add($"{prefix} {option.Name}", $"</{prefix} {option.Name}:{commandId}>");
            return;
        }

        foreach (var opt in option.Options)
            ProcessOption(opt, $"{prefix} {opt.Name}", commandId, result);
    }
}
