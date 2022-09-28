using Discord.Commands;
using GrillBot.App.Services.Channels;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.Help;

namespace GrillBot.App.Actions.Api.V1.Command;

public class GetCommandsHelp : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private CommandService CommandService { get; }
    private ChannelService ChannelService { get; }
    private IServiceProvider ServiceProvider { get; }

    private string Prefix { get; }

    public GetCommandsHelp(ApiRequestContext apiContext, IDiscordClient discordClient, CommandService commandService, ChannelService channelService, IServiceProvider serviceProvider,
        IConfiguration configuration) : base(apiContext)
    {
        DiscordClient = discordClient;
        CommandService = commandService;
        ChannelService = channelService;
        ServiceProvider = serviceProvider;

        Prefix = configuration.GetValue<string>("Discord:Commands:Prefix");
    }

    public async Task<List<CommandGroup>> ProcessAsync()
    {
        var loggedUser = await DiscordClient.FindUserAsync(ApiContext.GetUserId());
        var result = new List<CommandGroup>();

        foreach (var module in CommandService.Modules.Where(o => o.Commands.Count > 0))
        {
            var group = await ProcessGroupAsync(loggedUser, module);
            if (group != null) result.Add(group);
        }

        return result;
    }

    private async Task<CommandGroup> ProcessGroupAsync(IUser loggedUser, ModuleInfo module)
    {
        var group = new CommandGroup
        {
            Description = FormatHelper.FormatCommandDescription(module.Summary, Prefix, true),
            GroupName = module.Name
        };

        var mutualGuilds = await DiscordClient.FindMutualGuildsAsync(loggedUser.Id);
        foreach (var guild in mutualGuilds)
        {
            var lastMessage = await ChannelService.GetLastMsgFromUserAsync(guild, loggedUser);
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
                        Parameters = command.Parameters.Where(o => !string.IsNullOrEmpty(o.Name)).Select(o => o.FormatParameter()).ToList()
                    };
                    group.Commands.Add(cmd);
                }

                cmd.Guilds.Add(guild.Name);
            }
        }

        return group.Commands.Count > 0 ? group : null;
    }
}
