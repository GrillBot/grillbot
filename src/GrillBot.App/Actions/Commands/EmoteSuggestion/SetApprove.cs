using GrillBot.App.Helpers;
using GrillBot.App.Managers.EmoteSuggestion;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;

namespace GrillBot.App.Actions.Commands.EmoteSuggestion;

public class SetApprove : CommandAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMessageCacheManager MessageCache { get; }
    private IDiscordClient DiscordClient { get; }
    private ITextsManager Texts { get; }
    private EmoteSuggestionHelper Helper { get; }

    public SetApprove(GrillBotDatabaseBuilder databaseBuilder, IMessageCacheManager messageCache, IDiscordClient discordClient, ITextsManager texts,
        EmoteSuggestionHelper helper)
    {
        DatabaseBuilder = databaseBuilder;
        MessageCache = messageCache;
        DiscordClient = discordClient;
        Texts = texts;
        Helper = helper;
    }

    public async Task ProcessAsync(bool approved)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var interaction = (IComponentInteraction)Context.Interaction;
        var suggestion = await repository.EmoteSuggestion.FindSuggestionByMessageId(Context.Guild.Id, interaction.Message.Id);
        switch (suggestion)
        {
            case null:
                await interaction.UpdateAsync(msg => msg.Components = null);
                return;
            case { VoteFinished: false, VoteEndsAt: null }:
                suggestion.ApprovedForVote = approved;
                break;
        }

        await UpdateSuggestionMessageAsync(suggestion);
        await repository.CommitAsync();
        await interaction.DeferAsync();
    }

    private async Task UpdateSuggestionMessageAsync(Database.Entity.EmoteSuggestion suggestion)
    {
        var message = await MessageCache.GetAsync(suggestion.SuggestionMessageId.ToUlong(), Context.Channel);
        if (message is not IUserMessage userMessage) return;

        var fromUser = await DiscordClient.FindUserAsync(suggestion.FromUserId.ToUlong());
        await userMessage.ModifyAsync(msg =>
        {
            msg.Embed = new EmoteSuggestionEmbedBuilder(Texts).Build(suggestion, fromUser!, Locale);
            msg.Components = suggestion is { ApprovedForVote: true, VoteFinished: true } ? null : Helper.CreateApprovalButtons(Locale);
        });
    }
}
