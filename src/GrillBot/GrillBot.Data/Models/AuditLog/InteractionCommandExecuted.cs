using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GrillBot.Common.Extensions;

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
    public int Duration { get; set; }
    public string Exception { get; set; }

    [JsonIgnore]
    public string FullName => $"{Name} ({ModuleName}/{MethodName})";

    public InteractionCommandExecuted()
    {
    }

    public InteractionCommandExecuted(ICommandInfo commandInfo, IResult result, int duration)
    {
        Name = commandInfo.Name;
        ModuleName = commandInfo.Module.Name;
        MethodName = commandInfo.MethodName.Replace("Async", "", StringComparison.InvariantCultureIgnoreCase);
        Duration = duration;

        if (result == null)
            return;

        IsSuccess = result.IsSuccess;
        CommandError = result.Error;
        ErrorReason = result.ErrorReason;

        if (!result.IsSuccess && result is ExecuteResult { Exception: { } } executeResult)
            Exception = executeResult.Exception.ToString();
    }

    public InteractionCommandExecuted(ICommandInfo commandInfo, SocketSlashCommand interaction, IResult result, int duration)
        : this(commandInfo, result, duration)
    {
        HasResponded = interaction.HasResponded;
        IsValidToken = interaction.IsValidToken;

        Parameters = interaction.Data.Options.Flatten(o => o.Options)
            .Where(o => o.Type != ApplicationCommandOptionType.SubCommand && o.Type != ApplicationCommandOptionType.SubCommandGroup)
            .Select(o => new InteractionCommandParameter(o))
            .ToList();
    }

    public InteractionCommandExecuted(ICommandInfo commandInfo, SocketMessageCommand interaction, IResult result, int duration)
        : this(commandInfo, result, duration)
    {
        HasResponded = interaction.HasResponded;
        IsValidToken = interaction.IsValidToken;
        Parameters = new List<InteractionCommandParameter> { new(interaction.Data) };
    }

    public InteractionCommandExecuted(ICommandInfo commandInfo, SocketUserCommand interaction, IResult result, int duration)
        : this(commandInfo, result, duration)
    {
        HasResponded = interaction.HasResponded;
        IsValidToken = interaction.IsValidToken;
        Parameters = new List<InteractionCommandParameter> { new(interaction.Data) };
    }

    public InteractionCommandExecuted(ICommandInfo commandInfo, SocketMessageComponent component, IResult result, int duration)
        : this(commandInfo, result, duration)
    {
        HasResponded = component.HasResponded;
        IsValidToken = component.IsValidToken;

        Parameters = new List<InteractionCommandParameter>()
        {
            new() { Name = "CustomId", Type = "String", Value = component.Data.CustomId },
            new() { Name = "Type", Type = "String", Value = component.Data.Type.ToString() }
        };
    }

    public InteractionCommandExecuted(ICommandInfo commandInfo, SocketModal modal, IResult result, int duration)
        : this(commandInfo, result, duration)
    {
        HasResponded = modal.HasResponded;
        IsValidToken = modal.IsValidToken;

        Parameters = new List<InteractionCommandParameter>()
        {
            new() { Name = "ModalCustomId", Type = "String", Value = modal.Data.CustomId }
        };

        Parameters.AddRange(
            modal.Data.Components.SelectMany((component, index) => new[]
            {
                new InteractionCommandParameter { Name = $"ModalComponent({index}).CustomId", Type = "String", Value = component.CustomId },
                new InteractionCommandParameter { Name = $"ModalComponent({index}).Type", Type = "String", Value = component.Type.ToString() },
                component.Values?.Count > 0 ? new InteractionCommandParameter { Name = $"ModalComponent({index}).Values", Type = "String", Value = string.Join(", ", component.Values) } : null,
                !string.IsNullOrEmpty(component.Value) ? new InteractionCommandParameter { Name = $"ModalComponent({index}).Value", Type = "String", Value = component.Value } : null
            }).Where(o => o != null)
        );
    }

    [OnSerializing]
    internal void OnSerializing(StreamingContext _)
    {
        if (Parameters?.Count == 0) Parameters = null;
    }

    public static InteractionCommandExecuted Create(IDiscordInteraction interaction, ICommandInfo commandInfo, IResult result, int duration)
    {
        return interaction switch
        {
            SocketSlashCommand slashCommand => new InteractionCommandExecuted(commandInfo as SlashCommandInfo, slashCommand, result, duration),
            SocketMessageCommand messageCommand => new InteractionCommandExecuted(commandInfo as MessageCommandInfo, messageCommand, result, duration),
            SocketUserCommand userCommand => new InteractionCommandExecuted(commandInfo as UserCommandInfo, userCommand, result, duration),
            SocketMessageComponent component => new InteractionCommandExecuted(commandInfo as ComponentCommandInfo, component, result, duration),
            SocketModal modal => new InteractionCommandExecuted(commandInfo as ModalCommandInfo, modal, result, duration),
            _ => throw new NotSupportedException("Unsupported interaction type")
        };
    }
}
