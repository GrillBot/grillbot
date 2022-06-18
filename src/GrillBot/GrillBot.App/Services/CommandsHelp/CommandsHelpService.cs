using Discord.Commands;
using GrillBot.Data.Models.API.Help;
using GrillBot.App.Helpers;
using GrillBot.Data.Extensions;
using GrillBot.App.Services.Channels;

namespace GrillBot.App.Services.CommandsHelp;

/// <summary>
/// Service for generating commands for GrillBot commands.
/// </summary>
public class CommandsHelpService
{
    private DiscordSocketClient DiscordClient { get; }
    private CommandService CommandService { get; }
    private ChannelService ChannelService { get; }
    private IServiceProvider ServiceProvider { get; }

    private string Prefix { get; }

    public CommandsHelpService(DiscordSocketClient discordClient, CommandService commandService, ChannelService channelService,
        IServiceProvider provider, IConfiguration configuration)
    {
        DiscordClient = discordClient;
        CommandService = commandService;
        ChannelService = channelService;
        ServiceProvider = provider;

        Prefix = configuration.GetValue<string>("Discord:Commands:Prefix");
    }

    public async Task<List<CommandGroup>> GetHelpAsync(ulong loggedUserId, CancellationToken cancellationToken)
    {
        var loggedUser = await DiscordClient.FindUserAsync(loggedUserId, cancellationToken);
        var result = new List<CommandGroup>();

        foreach (var module in CommandService.Modules.Where(o => o.Commands.Count > 0))
        {
            var group = await GetTextBasedGroupAsync(loggedUser, module, cancellationToken);
            if (group != null) result.Add(group);
        }

        return result;
    }

    private async Task<CommandGroup> GetTextBasedGroupAsync(IUser loggedUser, ModuleInfo module, CancellationToken cancellationToken)
    {
        var group = new CommandGroup
        {
            Description = FormatHelper.FormatCommandDescription(module.Summary, Prefix, true),
            GroupName = module.Name
        };

        foreach (var guild in DiscordClient.FindMutualGuilds(loggedUser.Id))
        {
            var lastMessage = await ChannelService.GetLastMsgFromUserAsync(guild, loggedUser, cancellationToken);
            if (lastMessage == null) continue;
            var context = new CommandContext(DiscordClient, lastMessage);

            var availableCommands = await module.Commands.FindAllAsync(async cmd => (await cmd.CheckPreconditionsAsync(context, ServiceProvider)).IsSuccess);
            foreach (var command in availableCommands)
            {
                var fullCommand = command.GetCommandFormat(Prefix);
                var cmd = group.Commands.Find(o => o.CommandId == fullCommand);

                if (cmd == null)
                {
                    cmd = new TextBasedCommand
                    {
                        CommandId = fullCommand,
                        Command = Prefix + command.Aliases[0],
                        Aliases = command.GetAliases(Prefix).ToList(),
                        Description = FormatHelper.FormatCommandDescription(command.Summary, Prefix, true),
                        Guilds = new List<string>(),
                        Parameters = command.Parameters.Where(o => !string.IsNullOrEmpty(o.Name)).Select(o => FormatHelper.FormatParameter(o.Name, o.IsOptional)).ToList()
                    };
                    group.Commands.Add(cmd);
                }

                cmd.Guilds.Add(guild.Name);
            }
        }

        return group.Commands.Count > 0 ? group : null;
    }
}
