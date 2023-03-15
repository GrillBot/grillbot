using Discord.Net;
using GrillBot.App.Helpers;
using GrillBot.App.Managers.EmoteSuggestion;
using GrillBot.App.Modules.Implementations.Suggestion;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Exceptions;
using GrillBot.Database.Services.Repository;
using CacheManagers = GrillBot.Cache.Services.Managers;

namespace GrillBot.App.Actions.Commands.EmoteSuggestion;

public class FormSubmitted : CommandAction
{
    private CacheManagers.EmoteSuggestionManager CacheManager { get; }
    private ITextsManager Texts { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private EmoteSuggestionHelper Helper { get; }

    private string Filename { get; set; } = null!;
    private byte[] DataContent { get; set; } = null!;
    private Database.Entity.Guild GuildData { get; set; } = null!;

    public FormSubmitted(CacheManagers.EmoteSuggestionManager cacheManager, ITextsManager texts, GrillBotDatabaseBuilder databaseBuilder, EmoteSuggestionHelper helper)
    {
        CacheManager = cacheManager;
        Texts = texts;
        DatabaseBuilder = databaseBuilder;
        Helper = helper;
    }

    public async Task ProcessAsync(string sessionId, EmoteSuggestionModal modalData)
    {
        try
        {
            await using var repository = DatabaseBuilder.CreateRepository();
            await InitAsync(repository, sessionId);

            var entity = await CreateEntityAsync(repository, modalData);
            await SendAsync(entity);
            await SendPrivateMessageAsync(null);
            await repository.CommitAsync();
        }
        catch (Exception ex) when (ex is ValidationException or NotFoundException)
        {
            await SendPrivateMessageAsync(ex);
        }
    }

    private async Task InitAsync(GrillBotRepository repository, string sessionId)
    {
        var metadata = await CacheManager.PopAsync(sessionId);
        if (metadata == null)
            throw new NotFoundException(Texts["SuggestionModule/SessionExpired", Locale]);

        Filename = metadata.Value.filename;
        DataContent = metadata.Value.dataContent;
        GuildData = (await repository.Guild.FindGuildAsync(Context.Guild))!;
    }

    private async Task<Database.Entity.EmoteSuggestion> CreateEntityAsync(GrillBotRepository repository, EmoteSuggestionModal modalData)
    {
        var entity = new Database.Entity.EmoteSuggestion
        {
            CreatedAt = DateTime.Now,
            EmoteName = modalData.EmoteName,
            FromUserId = Context.User.Id.ToString(),
            GuildId = Context.Guild.Id.ToString(),
            Description = modalData.EmoteDescription,
            Filename = Filename,
            ImageData = DataContent
        };

        await repository.AddAsync(entity);
        await repository.User.GetOrCreateUserAsync(Context.User);
        await repository.GuildUser.GetOrCreateGuildUserAsync(await GetExecutingUserAsync());

        return entity;
    }

    private async Task SendAsync(Database.Entity.EmoteSuggestion entity)
    {
        var suggestionChannel = await Helper.FindEmoteSuggestionsChannelAsync(Context.Guild, GuildData, false, Locale);
        var approvalButtons = Helper.CreateApprovalButtons(Locale);
        var embed = new EmoteSuggestionEmbedBuilder(Texts).Build(entity, Context.User, Locale);

        // ReSharper disable once ConvertToUsingDeclaration
        using (var attachment = new FileAttachment(new MemoryStream(DataContent), entity.Filename))
        {
            var message = await suggestionChannel.SendFileAsync(attachment, embed: embed, components: approvalButtons);
            entity.SuggestionMessageId = message.Id.ToString();
        }
    }

    private async Task SendPrivateMessageAsync(Exception? exception)
    {
        try
        {
            var messageContent = exception?.Message ?? Texts["SuggestionModule/PrivateMessageSuggestionComplete", Locale];
            await Context.User.SendMessageAsync(messageContent);
        }
        catch (HttpException ex)
        {
            // User have blocked DMs from bots. User's problem, not ours.
            if (ex.DiscordCode != DiscordErrorCode.CannotSendMessageToUser)
                throw;
        }
    }
}
