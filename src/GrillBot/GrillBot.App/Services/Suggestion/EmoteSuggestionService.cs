using GrillBot.App.Modules.Implementations.Suggestion;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.Suggestion;
using GrillBot.Database.Enums;
using System.Net.Http;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common;
using GrillBot.Common.Helpers;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Services.Suggestion;

public class EmoteSuggestionService
{
    private SuggestionSessionService SessionService { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }
    private MessageCacheManager MessageCacheManager { get; }

    public EmoteSuggestionService(SuggestionSessionService sessionService, GrillBotDatabaseBuilder databaseBuilder,
        IDiscordClient discordClient, MessageCacheManager messageCacheManager)
    {
        SessionService = sessionService;
        DatabaseBuilder = databaseBuilder;
        DiscordClient = discordClient;
        MessageCacheManager = messageCacheManager;
    }

    public void InitSession(string suggestionId, object data)
        => SessionService.InitSuggestion(suggestionId, SuggestionType.VotableEmote, data);

    public async Task ProcessSessionAsync(string suggestionId, IGuild guild, IGuildUser user, EmoteSuggestionModal modalData)
    {
        var metadata = SessionService.PopMetadata(SuggestionType.VotableEmote, suggestionId);
        if (metadata == null)
            throw new NotFoundException("Nepodařilo se dohledat všechna data k tomuto návrhu. Podej prosím návrh znovu.");

        var entity = new Database.Entity.EmoteSuggestion
        {
            CreatedAt = DateTime.Now,
            EmoteName = modalData.EmoteName,
            FromUserId = user.Id.ToString(),
            GuildId = guild.Id.ToString(),
            Description = modalData.EmoteDescription
        };

        await SetEmoteDataAsync(metadata, entity);
        await TrySendSuggestionAsync(guild, entity, user);
    }

    private static async Task SetEmoteDataAsync(SuggestionMetadata metadata, Database.Entity.EmoteSuggestion entity)
    {
        switch (metadata.Data)
        {
            case Emote emote:
            {
                using var httpClient = new HttpClient();

                entity.Filename = emote.Name + Path.GetExtension(Path.GetFileName(emote.Url));
                entity.ImageData = await httpClient.GetByteArrayAsync(emote.Url);
                break;
            }
            case IAttachment attachment:
                entity.Filename = attachment.Filename;

                var imageData = await attachment.DownloadAsync();
                if (imageData == null)
                    throw new GrillBotException($"Nepodařilo se stáhnout potřebná data pro emote.");

                entity.ImageData = imageData;
                break;
        }
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
            var embed = BuildSuggestionEmbed(entity, author);
            var attachment = new FileAttachment(ms, entity.Filename);
            var message = await channel.SendFileAsync(attachment, embed: embed, components: components);

            entity.SuggestionMessageId = message.Id.ToString();
        }

        await repository.AddAsync(entity);
        await repository.CommitAsync();
    }

    private static MessageComponent BuildApprovalButtons()
    {
        return new ComponentBuilder()
            .WithButton("Schválit", "emote_suggestion_approve:true", ButtonStyle.Success)
            .WithButton("Zamítnout", "emote_suggestion_approve:false", ButtonStyle.Danger)
            .Build();
    }

    private static async Task<ITextChannel> FindEmoteSuggestionsChannelAsync(IGuild guild, Database.Entity.Guild dbGuild, bool isFinish)
    {
        if (string.IsNullOrEmpty(dbGuild.EmoteSuggestionChannelId))
        {
            throw new ValidationException(isFinish
                ? $"Nepodařilo se najít kanál pro návrhy ({dbGuild.EmoteSuggestionChannelId})."
                : "Tvůj návrh na nelze nyní zpracovat, protože není určen kanál pro návrhy.");
        }

        var channel = await guild.GetTextChannelAsync(dbGuild.EmoteSuggestionChannelId.ToUlong());

        if (channel == null)
        {
            throw new ValidationException(isFinish
                ? $"Nepodařilo se najít kanál pro návrhy ({dbGuild.EmoteSuggestionChannelId})."
                : "Tvůj návrh na emote nelze nyní kvůli technickým důvodům zpracovat.");
        }

        return channel;
    }

    private static Embed BuildSuggestionEmbed(Database.Entity.EmoteSuggestion entity, IUser author)
    {
        var builder = new EmbedBuilder()
            .WithAuthor(author)
            .WithColor(Color.Blue)
            .WithTitle("Nový návrh na emote")
            .WithTimestamp(entity.CreatedAt)
            .AddField("Název emote", entity.EmoteName);

        if (!string.IsNullOrEmpty(entity.Description))
            builder = builder.AddField("Popis", entity.Description);

        if (entity.VoteMessageId != null)
        {
            builder.WithDescription(
                (entity.VoteFinished ? "Hlasování skončilo " : "Hlasování běží, skončí ") + entity.VoteEndsAt!.Value.ToCzechFormat()
            );

            if (entity.VoteFinished)
            {
                builder
                    .WithTitle("Dokončeno hlasování o novém emote")
                    .AddField("Komunitou schválen", FormatHelper.FormatBooleanToCzech(entity.CommunityApproved), true)
                    .AddField(Emojis.ThumbsUp.ToString(), entity.UpVotes, true)
                    .AddField(Emojis.ThumbsDown.ToString(), entity.DownVotes, true);
            }
        }
        else if (entity.ApprovedForVote != null)
        {
            builder.WithDescription(entity.ApprovedForVote.Value ? "Schválen k hlasování" : "Zamítnut k hlasování");
        }

        return builder.Build();
    }

    public async Task SetApprovalStateAsync(IComponentInteraction interaction, bool approved, ISocketMessageChannel channel)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var suggestion = await repository.EmoteSuggestion.FindSuggestionByMessageId(interaction.GuildId!.Value, interaction.Message.Id);
        if (suggestion == null)
        {
            await interaction.UpdateAsync(msg => msg.Components = null);
            await interaction.DeferAsync();
            return;
        }

        await SetApprovalStateAsync(new List<Database.Entity.EmoteSuggestion> { suggestion }, approved, channel);
        await repository.CommitAsync();
        await interaction.DeferAsync();
    }

    private async Task SetApprovalStateAsync(IEnumerable<Database.Entity.EmoteSuggestion> suggestions, bool approved, IMessageChannel channel)
    {
        foreach (var suggestion in suggestions)
        {
            suggestion.ApprovedForVote ??= approved;

            var user = await DiscordClient.FindUserAsync(suggestion.FromUserId.ToUlong());
            if (await MessageCacheManager.GetAsync(suggestion.SuggestionMessageId.ToUlong(), channel) is IUserMessage message)
            {
                await message.ModifyAsync(msg =>
                {
                    msg.Embed = BuildSuggestionEmbed(suggestion, user);
                    msg.Components = suggestion.ApprovedForVote == true ? null : BuildApprovalButtons();
                });
            }
        }
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
                msg.Embed = BuildSuggestionEmbed(suggestion, fromUser);
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

    private static async Task<IUserMessage> SendSuggestionWithEmbedAsync(Database.Entity.EmoteSuggestion suggestion, IMessageChannel channel, string msg = null, Embed embed = null)
    {
        using (var ms = new MemoryStream(suggestion.ImageData))
        {
            var attachment = new FileAttachment(ms, suggestion.Filename);
            var allowedMentions = new AllowedMentions(AllowedMentionTypes.None);

            return await channel.SendFileAsync(attachment, msg, embed: embed, allowedMentions: allowedMentions);
        }
    }

    public async Task<string> ProcessJobAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var report = new StringBuilder();
        foreach (var guild in await DiscordClient.GetGuildsAsync())
        {
            var suggestions = await repository.EmoteSuggestion.FindSuggestionsForFinishAsync(guild);
            if (suggestions.Count == 0)
                continue;

            foreach (var suggestion in suggestions)
            {
                var suggestionReport = await FinishVoteForSuggestionAsync(guild, repository, suggestion);
                report.AppendLine(suggestionReport);
            }

            await repository.CommitAsync();
        }

        return report.ToString();
    }

    private async Task<string> FinishVoteForSuggestionAsync(IGuild guild, GrillBotRepository repository, Database.Entity.EmoteSuggestion suggestion)
    {
        try
        {
            var guildData = await repository.Guild.FindGuildAsync(guild);
            var suggestionsChannel = await FindEmoteSuggestionsChannelAsync(guild, guildData, true);

            if (string.IsNullOrEmpty(guildData!.VoteChannelId))
                throw new ValidationException($"Není nastaven kanál pro hlasování ({guildData.VoteChannelId})");
            var voteChannel = await guild.GetTextChannelAsync(guildData.VoteChannelId.ToUlong());
            if (voteChannel == null)
                throw new ValidationException($"Nepodařilo se najít kanál pro hlasování ({guildData.VoteChannelId})");

            if (await MessageCacheManager.GetAsync(suggestion.VoteMessageId!.ToUlong(), voteChannel, forceReload: true) is not IUserMessage message)
                return CreateJobReport(suggestion, "Nepodařilo se najít hlasovací zprávu.");

            var thumbsUpReactions = await message.GetReactionUsersAsync(Emojis.ThumbsUp, int.MaxValue).FlattenAsync();
            var thumbsDownReactions = await message.GetReactionUsersAsync(Emojis.ThumbsDown, int.MaxValue).FlattenAsync();

            suggestion.UpVotes = thumbsUpReactions.Count(o => o.IsUser());
            suggestion.DownVotes = thumbsDownReactions.Count(o => o.IsUser());
            suggestion.CommunityApproved = suggestion.UpVotes > suggestion.DownVotes;
            suggestion.VoteFinished = true;

            var fromUser = await DiscordClient.FindUserAsync(suggestion.FromUserId.ToUlong());
            await SendSuggestionWithEmbedAsync(suggestion, suggestionsChannel, embed: BuildSuggestionEmbed(suggestion, fromUser));
            await message.DeleteAsync();
            return CreateJobReport(suggestion, $"Úspěšně dokončen. ({suggestion.UpVotes}/{suggestion.DownVotes})");
        }
        catch (ValidationException ex)
        {
            return CreateJobReport(suggestion, ex.Message);
        }
    }

    private static string CreateJobReport(Database.Entity.EmoteSuggestion suggestion, string result)
        => $"Id:{suggestion.Id}, Guild:{suggestion.Guild!.Name}, FromUser:{suggestion.FromUser!.User!.Username}, EmoteName:{suggestion.EmoteName}, Result:{result}";
}
