using GrillBot.App.Helpers;
using GrillBot.Data.Models.API.Help;

namespace GrillBot.App.Services.CommandsHelp.Parsers;

public class RubbergodHelpParser : IHelpParser
{
    public List<CommandGroup> Parse(JArray json)
    {
        return json.Select(o => o as JObject)
            .Where(o => o != null)
            .Select(o => ProcessGroup(o))
            .ToList();
    }

    private static CommandGroup ProcessGroup(JObject group)
    {
        var commandGroup = new CommandGroup()
        {
            Description = FormatHelper.FormatCommandDescription(group.Value<string>("description"), "", true),
            GroupName = group.Value<string>("groupName")
        };

        commandGroup.Commands.AddRange(
            group.Value<JArray>("commands").Select(o => ProcessCommand(o as JObject))
        );

        return commandGroup;
    }

    private static TextBasedCommand ProcessCommand(JObject command)
    {
        var commandName = command.Value<string>("command");
        var signature = command.Value<string>("signature").Trim();

        return new TextBasedCommand()
        {
            Aliases = command.Value<JArray>("aliases").Select(o => o.ToObject<string>()).ToList(),
            Command = commandName,
            CommandId = $"{commandName} {signature}",
            Description = FormatHelper.FormatCommandDescription(command.Value<string>("description"), "", true),
            Parameters = ParseParameters(signature)
        };
    }

    private static List<string> ParseParameters(string signature)
    {
        if (string.IsNullOrEmpty(signature))
            return new();

        signature = FixAsterisks(signature);
        return GetParams(signature);
    }

    private static string FixAsterisks(string signature)
    {
        if (signature.StartsWith("*"))
            signature = $"<{signature[1..]}";

        if (signature.EndsWith("*"))
            signature = $"{signature[0..^1]}>";

        return signature;
    }

    private static List<string> GetParams(string signature)
    {
        var fields = signature.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var result = new List<string>();

        var paramBuilder = new StringBuilder();
        char paramType = ' ';
        foreach (var field in fields)
        {
            if (field.StartsWith("["))
            {
                if (field.EndsWith("]"))
                {
                    result.Add(field);
                }
                else
                {
                    paramType = '[';
                    paramBuilder.Append(field).Append(' ');
                }
            }
            else if ((field.EndsWith("]") && paramType == '[') || field.EndsWith(">") && paramType == '<')
            {
                paramBuilder.Append(field[..^1]);
                paramType = ' ';
                result.Add(paramBuilder.ToString());
                paramBuilder.Clear();
            }
            else if (field.StartsWith("<"))
            {
                if (field.EndsWith(">"))
                {
                    result.Add(field[1..^1]);
                }
                else
                {
                    paramType = '<';
                    paramBuilder.Append(field[1..]).Append(' ');
                }
            }
            else
            {
                paramBuilder.Append(field).Append(' ');
            }
        }

        return result;
    }
}
