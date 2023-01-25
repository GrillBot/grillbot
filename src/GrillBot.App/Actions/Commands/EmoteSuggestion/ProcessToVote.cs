using GrillBot.App.Helpers;
using GrillBot.App.Managers.EmoteSuggestion;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Data.Exceptions;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Actions.Commands.EmoteSuggestion;

public class ProcessToVote : CommandAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }
    private EmoteSuggestionHelper Helper { get; }
    private IMessageCacheManager MessageCache { get; }
    private IDiscordClient DiscordClient { get; }

    private Database.Entity.Guild GuildData { get; set; } = null!;
    private ITextChannel VoteChannel { get; set; } = null!;
    private ITextChannel SuggestionChannel { get; set; } = null!;

    public ProcessToVote(GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts, EmoteSuggestionHelper helper, IMessageCacheManager messageCache, IDiscordClient discordClient)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
        Helper = helper;
        MessageCache = messageCache;
        DiscordClient = discordClient;
    }

    public async Task ProcessAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        await InitAsync(repository);

        var suggestions = await repository.EmoteSuggestion.FindSuggestionsForProcessingAsync(Context.Guild);
        if (suggestions.Count == 0) throw new GrillBotException(Texts["SuggestionModule/NoForVote", Locale]);

        suggestions = suggestions.FindAll(o => o.ApprovedForVote == true);
        if (suggestions.Count == 0) throw new GrillBotException(Texts["SuggestionModule/NoApprovedForVote", Locale]);

        foreach (var suggestion in suggestions)
        {
            await ProcessSuggestionForVoteAsync(suggestion);
            await UpdateSuggestionMessageAsync(suggestion);
        }

        await repository.CommitAsync();
    }

    private async Task InitAsync(GrillBotRepository repository)
    {
        GuildData = (await repository.Guild.FindGuildAsync(Context.Guild))!;

        if (string.IsNullOrEmpty(GuildData.VoteChannelId))
            throw new GrillBotException(Texts["SuggestionModule/VoteChannelNotDefined", Locale]);
        VoteChannel = await Context.Guild.GetTextChannelAsync(GuildData.VoteChannelId.ToUlong());
        if (VoteChannel == null)
            throw new NotFoundException(Texts["SuggestionModule/VoteChannelNotFound", Locale]);
        SuggestionChannel = await Helper.FindEmoteSuggestionsChannelAsync(Context.Guild, GuildData, false, Locale);
    }

    private async Task ProcessSuggestionForVoteAsync(Database.Entity.EmoteSuggestion suggestion)
    {
        suggestion.VoteEndsAt = DateTime.Now.AddDays(7);

        var voteMessage = new StringBuilder("Hlasování o novém emote s návem **")
            .Append(suggestion.EmoteName)
            .AppendLine("**")
            .Append("Hlasování skončí **")
            .Append(suggestion.VoteEndsAt!.Value.ToCzechFormat())
            .AppendLine("**")
            .ToString();

        var message = await EmoteSuggestionHelper.SendSuggestionWithEmbedAsync(suggestion, VoteChannel, voteMessage);
        await message.AddReactionsAsync(Emojis.VoteEmojis);
        suggestion.VoteMessageId = message.Id.ToString();
    }

    private async Task UpdateSuggestionMessageAsync(Database.Entity.EmoteSuggestion suggestion)
    {
        var message = await MessageCache.GetAsync(suggestion.SuggestionMessageId.ToUlong(), SuggestionChannel);
        if (message is not IUserMessage userMessage) return;

        var fromUser = await DiscordClient.FindUserAsync(suggestion.FromUserId.ToUlong());
        await userMessage.ModifyAsync(msg =>
        {
            msg.Embed = new EmoteSuggestionEmbedBuilder(Texts).Build(suggestion, fromUser!, Locale);
            msg.Components = null;
        });
    }
}
