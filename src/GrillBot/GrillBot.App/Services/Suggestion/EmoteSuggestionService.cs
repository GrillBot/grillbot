using GrillBot.App.Modules.Implementations.Suggestion;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.Suggestion;
using GrillBot.Database.Enums;
using System.Net.Http;

namespace GrillBot.App.Services.Suggestion;

public class EmoteSuggestionService
{
    private SuggestionSessionService SessionService { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }

    public EmoteSuggestionService(SuggestionSessionService sessionService, GrillBotDatabaseBuilder databaseBuilder,
        IDiscordClient discordClient)
    {
        SessionService = sessionService;
        DatabaseBuilder = databaseBuilder;
        DiscordClient = discordClient;
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

        if (string.IsNullOrEmpty(guildData.EmoteSuggestionChannelId))
            throw new ValidationException("Tvůj návrh na nelze nyní zpracovat, protože není určen kanál pro návrhy.");

        var channel = await guild.GetTextChannelAsync(guildData.EmoteSuggestionChannelId.ToUlong());

        if (channel == null)
            throw new ValidationException("Tvůj návrh na emote nelze nyní kvůli technickým důvodům zpracovat.");

        // ReSharper disable once ConvertToUsingDeclaration
        using (var ms = new MemoryStream(entity.ImageData))
        {
            var components = new ComponentBuilder()
                .WithButton("Schválit", "emote_suggestion_approve:true", ButtonStyle.Success)
                .WithButton("Zamítnout", "emote_suggestion_approve:false", ButtonStyle.Danger)
                .Build();
            var embed = BuildSuggestionEmbed(entity, author);
            var attachment = new FileAttachment(ms, entity.Filename);
            var message = await channel.SendFileAsync(attachment, embed: embed, components: components);

            entity.SuggestionMessageId = message.Id.ToString();
        }

        await repository.AddAsync(entity);
        await repository.CommitAsync();
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

        if (entity.ApprovedForVote != null)
            builder.WithDescription(entity.ApprovedForVote.Value ? "Schválen k hlasování" : "Zamítnut k hlasování");

        return builder
            .Build();
    }

    public async Task SetApprovalStateAsync(IComponentInteraction interaction, bool approved)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var suggestion = await repository.EmoteSuggestion.FindSuggestionByMessageId(interaction.Message.Id);
        if (suggestion == null)
        {
            await interaction.UpdateAsync(msg => msg.Components = null);
            return;
        }

        suggestion.ApprovedForVote = approved;
        await repository.CommitAsync();

        var user = await DiscordClient.FindUserAsync(suggestion.FromUserId.ToUlong());
        var embed = BuildSuggestionEmbed(suggestion, user);
        await interaction.UpdateAsync(msg => msg.Embed = embed);
    }
}
