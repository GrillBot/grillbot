using Discord.Interactions;
using GrillBot.App.Actions.Commands.Emotes.Suggestions;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.Core.Services.Emote.Models.Events.Suggestions;

namespace GrillBot.App.Modules.Interactions.Emote;

[Group("emote-suggestions", "Emote suggestions")]
[RequireUserPerms]
public class SuggestionModule(IServiceProvider serviceProvider) : InteractionsModuleBase(serviceProvider)
{
    [SlashCommand("create", "Creates emote suggestion.")]
    public async Task CreateSuggestionAsync(
        [
            Summary("reason", "Reason for suggestion"),
            Discord.Interactions.MaxLength(EmbedFieldBuilder.MaxFieldValueLength)
        ]
        string reason,
        [
            Summary("name", "Name of emote. Optional if emote is filled, required for attachments."),
            Discord.Interactions.MaxLength(255)
        ]
        string? emoteName = null,
        [Summary("emote", "Emote from another server, which you want to suggest.")]
        IEmote? emote = null,
        [Summary("attachment", "PNG or GIF image of emote")]
        IAttachment? attachment = null
    )
    {
        using var command = await GetCommandAsync<CreateEmoteSuggestionAction>();
        await command.Command.ProcessAsync(reason, emoteName, emote, attachment);

        if (string.IsNullOrEmpty(command.Command.ErrorMessage))
            await SetResponseAsync(GetText(nameof(CreateSuggestionAsync), "Success"));
        else
            await SetResponseAsync(command.Command.ErrorMessage);
    }

    [ComponentInteraction("suggestion_approve_for_vote:*:*", ignoreGroupNames: true)]
    public async Task SuggestionApprovalChangeClicked(Guid suggestionId, bool approved)
    {
        await SendViaRabbitAsync(EmoteSuggestionApprovalChangePayload.Create(suggestionId, approved, Context.User));
        await DeferAsync();
    }
}
