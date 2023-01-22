﻿using GrillBot.App.Modules.Implementations.Suggestion;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Data.Exceptions;
using GrillBot.Cache.Services.Managers;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Services.Suggestion;

public partial class EmoteSuggestionService
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }
    private IMessageCacheManager MessageCacheManager { get; }
    private IServiceProvider ServiceProvider { get; }

    public EmoteSuggestionService(GrillBotDatabaseBuilder databaseBuilder, IServiceProvider serviceProvider, IDiscordClient discordClient, IMessageCacheManager messageCacheManager)
    {
        DatabaseBuilder = databaseBuilder;
        DiscordClient = discordClient;
        MessageCacheManager = messageCacheManager;
        ServiceProvider = serviceProvider;
    }

    public async Task ProcessSessionAsync(string suggestionId, IGuild guild, IGuildUser user, EmoteSuggestionModal modalData)
    {
        using var scope = ServiceProvider.CreateScope();
        var cacheManager = scope.ServiceProvider.GetRequiredService<EmoteSuggestionManager>();

        var metadata = await cacheManager.PopAsync(suggestionId);
        if (metadata == null)
            throw new NotFoundException("Nepodařilo se dohledat všechna data k tomuto návrhu. Podej prosím návrh znovu.");

        var entity = new Database.Entity.EmoteSuggestion
        {
            CreatedAt = DateTime.Now,
            EmoteName = modalData.EmoteName,
            FromUserId = user.Id.ToString(),
            GuildId = guild.Id.ToString(),
            Description = modalData.EmoteDescription,
            Filename = metadata.Value.filename,
            ImageData = metadata.Value.dataContent
        };

        await TrySendSuggestionAsync(guild, entity, user);
    }

    private async Task TrySendSuggestionAsync(IGuild guild, Database.Entity.EmoteSuggestion entity, IGuildUser author)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var guildData = await repository.Guild.FindGuildAsync(guild);
        if (guildData == null) return;

        await repository.User.GetOrCreateUserAsync(author);
        await repository.GuildUser.GetOrCreateGuildUserAsync(author);

        var channel = await FindEmoteSuggestionsChannelAsync(guild, guildData, false);

        // ReSharper disable once ConvertToUsingDeclaration
        using (var ms = new MemoryStream(entity.ImageData))
        {
            var components = BuildApprovalButtons();
            var embed = new EmoteSuggestionEmbedBuilder(entity, author).Build();
            var attachment = new FileAttachment(ms, entity.Filename);
            var message = await channel.SendFileAsync(attachment, embed: embed, components: components);

            entity.SuggestionMessageId = message.Id.ToString();
        }

        await repository.AddAsync(entity);
        await repository.CommitAsync();
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
