using Discord.Interactions;
using GrillBot.App.Services.Discord;
using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Extensions;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Events.Create;

namespace GrillBot.App.Handlers.InteractionCommandExecuted;

public class AuditInteractionCommandHandler : IInteractionCommandExecutedEvent
{
    private readonly IRabbitMQPublisher _rabbitPublisher;

    public AuditInteractionCommandHandler(IRabbitMQPublisher rabbitPublisher)
    {
        _rabbitPublisher = rabbitPublisher;
    }

    public async Task ProcessAsync(ICommandInfo commandInfo, IInteractionContext context, IResult result)
    {
        if (!Init(result, context, out var duration)) return;

        var guildId = context.Guild?.Id.ToString();
        var channelId = context.Channel.Id.ToString();
        var userId = context.User.Id.ToString();

        var logRequest = new LogRequest(LogType.InteractionCommand, DateTime.UtcNow, guildId, userId, channelId)
        {
            InteractionCommand = CreateCommandRequest(commandInfo, context, result, duration)
        };

        await _rabbitPublisher.PublishAsync(new CreateItemsPayload(new() { logRequest }));
    }

    private static bool Init(IResult result, IInteractionContext context, out int duration)
    {
        duration = CommandsPerformanceCounter.TaskExists(context) ? CommandsPerformanceCounter.TaskFinished(context) : 0;
        return result.Error != InteractionCommandError.UnknownCommand;
    }

    private static InteractionCommandRequest CreateCommandRequest(ICommandInfo command, IInteractionContext context, IResult result, int duration)
    {
        var request = new InteractionCommandRequest
        {
            Duration = duration,
            IsSuccess = result.IsSuccess,
            Locale = context.Interaction.UserLocale,
            Name = command.Name,
            CommandError = (int?)result.Error,
            ErrorReason = result.ErrorReason,
            HasResponded = context.Interaction.HasResponded,
            MethodName = command.MethodName.Replace("Async", "", StringComparison.InvariantCultureIgnoreCase),
            ModuleName = command.Module.Name
        };

        if (!result.IsSuccess && result is ExecuteResult { Exception: not null } executeResult)
            request.Exception = executeResult.Exception.ToString();

        switch (context.Interaction)
        {
            case SocketSlashCommand slashCommand:
                request.IsValidToken = slashCommand.IsValidToken;
                request.Parameters = slashCommand.Data.Options.Flatten(o => o.Options)
                    .Where(o => o.Type != ApplicationCommandOptionType.SubCommand && o.Type != ApplicationCommandOptionType.SubCommandGroup)
                    .Select(CreateParameterRequest)
                    .ToList();
                break;
            case SocketMessageCommand messageCommand:
                {
                    request.IsValidToken = messageCommand.IsValidToken;

                    var message = messageCommand.Data.Message;
                    request.Parameters = new List<InteractionCommandParameterRequest>
                    {
                        CreateParameterRequest("Message", messageCommand.Data.Name, $"Message({message.Author.Username}, {message.CreatedAt.LocalDateTime.ToCzechFormat()}, {message.Content})")
                    };
                    break;
                }
            case SocketUserCommand userCommand:
                {
                    request.IsValidToken = userCommand.IsValidToken;

                    var user = userCommand.Data.Member;
                    request.Parameters = new List<InteractionCommandParameterRequest>
                    {
                        CreateParameterRequest("User", userCommand.Data.Name, $"User({user.Username}, {user.Id})")
                    };
                    break;
                }
            case SocketMessageComponent messageComponent:
                request.IsValidToken = messageComponent.IsValidToken;

                request.Parameters = new List<InteractionCommandParameterRequest>
                {
                    CreateParameterRequest("String", "CustomId", messageComponent.Data.CustomId),
                    CreateParameterRequest("String", "Type", messageComponent.Data.Type.ToString())
                };
                break;
            case SocketModal modal:
                request.IsValidToken = modal.IsValidToken;

                request.Parameters = new List<InteractionCommandParameterRequest>
                {
                    CreateParameterRequest("String", "ModalCustomId", modal.Data.CustomId)
                };

                request.Parameters.AddRange(modal.Data.Components.SelectMany((component, index) => new[]
                {
                    CreateParameterRequest("String", $"ModalComponent({index}).CustomId", component.CustomId),
                    CreateParameterRequest("String", $"ModalComponent({index}).Type", component.Type.ToString()),
                    component.Values.Count > 0 ? CreateParameterRequest("String", $"ModalComponent({index}).Values", string.Join(", ", component.Values)) : null,
                    !string.IsNullOrEmpty(component.Value) ? CreateParameterRequest("String", $"ModalComponent({index}).Value", component.Value) : null
                }).Where(o => o is not null).Select(o => o!));
                break;
        }

        return request;
    }

    private static string ConvertValue(ApplicationCommandOptionType type, object value)
    {
        switch (type)
        {
            case ApplicationCommandOptionType.Attachment:
                var attachment = (IAttachment)value;
                return $"Attachment({attachment.Filename}, {attachment.Size.Bytes()})";
            case ApplicationCommandOptionType.Boolean or ApplicationCommandOptionType.Integer or ApplicationCommandOptionType.Number or ApplicationCommandOptionType.String:
                return value.ToString() ?? "";
            case ApplicationCommandOptionType.Channel:
                var channel = (IChannel)value;
                return $"Channel({channel.Name}; {channel.GetChannelType()}, {channel.Id})";
            case ApplicationCommandOptionType.Role:
                var role = (IRole)value;
                return $"Role({role.Name}, {role.Id})";
            case ApplicationCommandOptionType.User:
                var user = (IUser)value;
                return $"User({user.Username}, {user.Id})";
            default:
                return "";
        }
    }

    private static InteractionCommandParameterRequest CreateParameterRequest(IApplicationCommandInteractionDataOption option)
    {
        var value = ConvertValue(option.Type, option.Value);
        return CreateParameterRequest(option.Type.ToString(), option.Name, value);
    }
    private static InteractionCommandParameterRequest CreateParameterRequest(string type, string name, string value)
    {
        return new InteractionCommandParameterRequest
        {
            Name = name,
            Type = type,
            Value = value
        };
    }
}
