using Discord.Interactions;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.Core.Exceptions;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
[DefaultMemberPermissions(GuildPermission.UseApplicationCommands | GuildPermission.ViewAuditLog)]
[Group("admin", "Administration commands")]
public class AdminModule(IServiceProvider serviceProvider) : InteractionsModuleBase(serviceProvider)
{
    [SlashCommand("clean_messages", "Deletes the last N messages (or until concrete message ID) from the channel.")]
    [RequireBotPermission(ChannelPermission.ManageMessages | ChannelPermission.ReadMessageHistory)]
    [DeferConfiguration(RequireEphemeral = true)]
    public async Task CleanAsync(string criterium, IGuildChannel? channel = null)
    {
        using var command = await GetCommandAsync<Actions.Commands.CleanChannelMessages>();

        var result = await command.Command.ProcessAsync(criterium, channel);
        await SetResponseAsync(result, secret: true);
    }

    [SlashCommand("send", "Send message to command")]
    [DeferConfiguration(RequireEphemeral = true)]
    public async Task SendMessageToChannelAsync(IGuildChannel channel, [Discord.Interactions.MaxLength(DiscordConfig.MaxMessageSize)] string? content = null, string? reference = null,
        IAttachment? attachment = null)
    {
        using var command = GetCommand<Actions.Commands.SendMessageToChannel>();

        try
        {
            await command.Command.ProcessAsync(channel, reference, content, new[] { attachment });
            await SetResponseAsync(Texts["ChannelModule/PostMessage/Success", Locale], secret: true);
        }
        catch (Exception ex) when (ex is ValidationException or NotFoundException)
        {
            await SetResponseAsync(ex.Message, secret: true);
        }
    }
}
