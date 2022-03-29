using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using GrillBot.Data.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GrillBot.Data.Models.AuditLog;

public class InteractionCommandExecuted
{
    public string Name { get; set; }
    public string ModuleName { get; set; }
    public string MethodName { get; set; }
    public List<InteractionCommandParameter> Parameters { get; set; }
    public bool HasResponded { get; set; }
    public bool IsValidToken { get; set; }
    public bool IsSuccess { get; set; }
    public InteractionCommandError? CommandError { get; set; }
    public string ErrorReason { get; set; }

    [JsonIgnore]
    public string FullName => $"{Name} ({ModuleName}/{MethodName})";

    public InteractionCommandExecuted() { }

    public InteractionCommandExecuted(ICommandInfo commandInfo, IResult result)
    {
        Name = commandInfo.Name;
        ModuleName = commandInfo.Module.Name;
        MethodName = commandInfo.MethodName.Replace("Async", "", StringComparison.InvariantCultureIgnoreCase);

        if (result != null)
        {
            IsSuccess = result.IsSuccess;
            CommandError = result.Error;
            ErrorReason = result.ErrorReason;
        }
    }

    public InteractionCommandExecuted(SlashCommandInfo commandInfo, SocketSlashCommand interaction, IResult result)
        : this(commandInfo, result)
    {
        HasResponded = interaction.HasResponded;
        IsValidToken = interaction.IsValidToken;

        Parameters = interaction.Data.Options.Flatten(o => o.Options)
            .Where(o => o.Type != ApplicationCommandOptionType.SubCommand && o.Type != ApplicationCommandOptionType.SubCommandGroup)
            .Select(o => new InteractionCommandParameter(o))
            .ToList();
    }

    public InteractionCommandExecuted(MessageCommandInfo commandInfo, SocketMessageCommand interaction, IResult result)
        : this(commandInfo, result)
    {
        HasResponded = interaction.HasResponded;
        IsValidToken = interaction.IsValidToken;
        Parameters = new() { new(interaction.Data) };
    }

    public InteractionCommandExecuted(UserCommandInfo commandInfo, SocketUserCommand interaction, IResult result)
        : this(commandInfo, result)
    {
        HasResponded = interaction.HasResponded;
        IsValidToken = interaction.IsValidToken;
        Parameters = new() { new(interaction.Data) };
    }

    public InteractionCommandExecuted(ComponentCommandInfo commandInfo, SocketMessageComponent component, IResult result)
        : this(commandInfo, result)
    {
        HasResponded = component.HasResponded;
        IsValidToken = component.IsValidToken;

        Parameters = new List<InteractionCommandParameter>()
        {
            new InteractionCommandParameter() { Name = "CustomId", Type = "String", Value = component.Data.CustomId },
            new InteractionCommandParameter() { Name = "Type", Type = "String", Value = component.Data.Type.ToString() }
        };
    }

    public InteractionCommandExecuted(ModalCommandInfo commandInfo, SocketModal modal, IResult result)
        : this(commandInfo, result)
    {
        HasResponded = modal.HasResponded;
        IsValidToken = modal.IsValidToken;

        Parameters = new List<InteractionCommandParameter>()
        {
            new InteractionCommandParameter() { Name = "ModalCustomId", Type = "String", Value = modal.Data.CustomId }
        };

        Parameters.AddRange(
            modal.Data.Components.SelectMany((component, index) => new[]
            {
                 new InteractionCommandParameter() { Name = $"ModalComponent({index}).CustomId", Type = "String", Value = component.CustomId },
                 new InteractionCommandParameter() { Name = $"ModalComponent({index}).Type", Type = "String", Value = component.Type.ToString() },
                 component.Values?.Count > 0 ? new InteractionCommandParameter() { Name = $"ModalComponent({index}).Values", Type = "String", Value = string.Join(", ", component.Values) } : null,
                 !string.IsNullOrEmpty(component.Value) ? new InteractionCommandParameter() { Name =  $"ModalComponent({index}).Value", Type = "String", Value = component.Value } : null
            }).Where(o => o != null)
        );
    }

    [OnSerializing]
    internal void OnSerializing(StreamingContext _)
    {
        if (Parameters?.Count == 0) Parameters = null;
    }

    public static InteractionCommandExecuted Create(IDiscordInteraction interaction, ICommandInfo commandInfo, IResult result)
    {
        if (interaction is SocketSlashCommand slashCommand)
            return new InteractionCommandExecuted(commandInfo as SlashCommandInfo, slashCommand, result);
        else if (interaction is SocketMessageCommand messageCommand)
            return new InteractionCommandExecuted(commandInfo as MessageCommandInfo, messageCommand, result);
        else if (interaction is SocketUserCommand userCommand)
            return new InteractionCommandExecuted(commandInfo as UserCommandInfo, userCommand, result);
        else if (interaction is SocketMessageComponent component)
            return new InteractionCommandExecuted(commandInfo as ComponentCommandInfo, component, result);
        else if (interaction is SocketModal modal)
            return new InteractionCommandExecuted(commandInfo as ModalCommandInfo, modal, result);

        throw new NotSupportedException("Unsupported interaction type");
    }
}
