using System.Diagnostics.CodeAnalysis;
using Discord.Interactions;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
[DefaultMemberPermissions(GuildPermission.UseApplicationCommands | GuildPermission.ViewAuditLog)]
[Group("admin", "Administration commands")]
[ExcludeFromCodeCoverage]
public class AdminModule : InteractionsModuleBase
{
    public AdminModule(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [SlashCommand("clean_messages", "Deletes the last N messages from the channel.")]
    [RequireBotPermission(ChannelPermission.ManageMessages | ChannelPermission.ReadMessageHistory)]
    [DeferConfiguration(RequireEphemeral = true)]
    public async Task CleanAsync(int count, ITextChannel channel = null)
    {
        using var command = GetCommand<Actions.Commands.CleanChannelMessages>();

        var result = await command.Command.ProcessAsync(count, channel);
        await SetResponseAsync(result, secret: true);
    }

    [SlashCommand("send", "Send message to command")]
    [DeferConfiguration(RequireEphemeral = true)]
    public async Task SendMessageToChannelAsync(ITextChannel channel, string content = null, string reference = null, IAttachment attachment = null)
    {
        using var command = GetCommand<Actions.Commands.SendMessageToChannel>();

        try
        {
            await command.Command.ProcessAsync(channel, reference, content, new[] { attachment });
            await SetResponseAsync(Texts["ChannelModule/PostMessage/Success", Locale], secret: true);
        }
        catch (ValidationException ex)
        {
            await SetResponseAsync(ex.Message, secret: true);
        }
    }
}
