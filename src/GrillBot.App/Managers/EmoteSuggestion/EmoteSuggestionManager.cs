using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Data.Exceptions;

namespace GrillBot.App.Managers.EmoteSuggestion;

public partial class EmoteSuggestionManager
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }
    private IMessageCacheManager MessageCacheManager { get; }

    public EmoteSuggestionManager(GrillBotDatabaseBuilder databaseBuilder, IDiscordClient discordClient, IMessageCacheManager messageCacheManager)
    {
        DatabaseBuilder = databaseBuilder;
        DiscordClient = discordClient;
        MessageCacheManager = messageCacheManager;
    }

    public async Task ProcessSuggestionsToVoteAsync(IGuild guild)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var guildData = await repository.Guild.FindGuildAsync(guild);
        var channel = await FindEmoteSuggestionsChannelAsync(guild, guildData, false);

        if (string.IsNullOrEmpty(guildData!.VoteChannelId))
            throw new GrillBotException("Nelze spustit hlasování o nových emotech, protože není definován kanál pro hlasování.");
        var voteChannel = await guild.GetTextChannelAsync(guildData.VoteChannelId.ToUlong());
        if (voteChannel == null)
            throw new GrillBotException("Nelze spustit hlasování o nových emotech, protože nebyl nalezen kanál pro hlasování.");

        var suggestions = await repository.EmoteSuggestion.FindSuggestionsForProcessingAsync(guild);
        if (suggestions.Count == 0)
            throw new GrillBotException("Neexistuje žádný schválený/zamítnutý návrh ke zpracování.");

        var approvedSuggestions = suggestions.FindAll(o => o.ApprovedForVote == true);
        if (approvedSuggestions.Count == 0)
            throw new ValidationException("Není žádný schválený návrh ke zpracování.");

        foreach (var suggestion in approvedSuggestions)
        {
            await ProcessSuggestionToVoteAsync(suggestion, voteChannel);

            // Once the command is executed, all proposals marked as approved cannot be changed.
            // Rejected proposals can be changed.
            if (await MessageCacheManager.GetAsync(suggestion.SuggestionMessageId.ToUlong(), channel) is not IUserMessage message)
                continue;

            var fromUser = await DiscordClient.FindUserAsync(suggestion.FromUserId.ToUlong());
            await message.ModifyAsync(msg =>
            {
                msg.Embed = new EmoteSuggestionEmbedBuilder(suggestion, fromUser).Build();
                msg.Components = null;
            });
        }

        await repository.CommitAsync();
    }

    private static async Task ProcessSuggestionToVoteAsync(Database.Entity.EmoteSuggestion suggestion, IMessageChannel voteChannel)
    {
        suggestion.VoteEndsAt = DateTime.Now.AddDays(7);

        var msg = new StringBuilder("Hlasování o novém emote s návem **").Append(suggestion.EmoteName).AppendLine("**")
            .Append("Hlasování skončí **").Append(suggestion.VoteEndsAt!.Value.ToCzechFormat()).AppendLine("**")
            .ToString();

        var message = await SendSuggestionWithEmbedAsync(suggestion, voteChannel, msg);
        await message.AddReactionsAsync(Emojis.VoteEmojis);
        suggestion.VoteMessageId = message.Id.ToString();
    }
}
