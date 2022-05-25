#pragma warning disable IDE0063 // Use simple 'using' statement

using GrillBot.App.Infrastructure;
using GrillBot.App.Modules.Implementations.Suggestion;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Extensions;
using GrillBot.Data.Models.Suggestion;
using GrillBot.Database.Enums;
using System.Net.Http;

namespace GrillBot.App.Services.Suggestion;

public class EmoteSuggestionService : ServiceBase
{
    private SuggestionSessionService SessionService { get; }

    public EmoteSuggestionService(SuggestionSessionService sessionService, GrillBotContextFactory dbFactory) : base(null, dbFactory, null, null)
    {
        SessionService = sessionService;
    }

    public void InitSession(string suggestionId, object data)
        => SessionService.InitSuggestion(suggestionId, SuggestionType.Emote, data);

    public async Task ProcessSessionAsync(string suggestionId, IGuild guild, IUser user, EmoteSuggestionModal modalData)
    {
        var metadata = SessionService.PopMetadata(SuggestionType.Emote, suggestionId);
        if (metadata == null)
            throw new NotFoundException("Nepodařilo se dohledat všechna data k tomuto návrhu. Podej prosím návrh znovu.");

        var dataBuilder = new StringBuilder()
            .Append("Nový návrh na emote od uživatele **").Append(user.GetFullName()).AppendLine("**")
            .Append("Název: **").Append(modalData.EmoteName).AppendLine("**");

        if (!string.IsNullOrEmpty(modalData.EmoteDescription))
            dataBuilder.AppendLine("Popis:").AppendLine("```").AppendLine(modalData.EmoteDescription).AppendLine("```");

        var entity = new Database.Entity.Suggestion()
        {
            CreatedAt = DateTime.Now,
            Data = dataBuilder.ToString(),
            Type = SuggestionType.Emote,
            GuildId = guild.Id.ToString()
        };

        await SetEmoteDataAsync(metadata, entity);
        await TrySendSuggestionAsync(guild, entity);
    }

    private static async Task SetEmoteDataAsync(SuggestionMetadata metadata, Database.Entity.Suggestion suggestion)
    {
        if (metadata.Data is Emote emote)
        {
            using var httpClient = new HttpClient();

            suggestion.BinaryDataFilename = emote.Name + Path.GetExtension(Path.GetFileName(emote.Url));
            suggestion.BinaryData = await httpClient.GetByteArrayAsync(emote.Url);
        }
        else if (metadata.Data is IAttachment attachment)
        {
            suggestion.BinaryDataFilename = attachment.Filename;
            suggestion.BinaryData = await attachment.DownloadAsync();
        }
    }

    public async Task TrySendSuggestionAsync(IGuild guild, Database.Entity.Suggestion suggestion)
    {
        var data = suggestion.Data +
            (suggestion.Id != default ? $"\nTenhle návrh vznikl {suggestion.CreatedAt.ToCzechFormat()}, ale nepovedlo se ho poslat." : "");

        try
        {
            using var dbContext = DbFactory.Create();
            var guildData = await dbContext.Guilds.FirstOrDefaultAsync(o => o.Id == guild.Id.ToString());

            if (string.IsNullOrEmpty(guildData.EmoteSuggestionChannelId))
                throw new ValidationException("Tvůj návrh na emote byl zařazen ke zpracování, ale kvůli technickým důvodům jej nelze nyní zpracovat.");

            var channel = await guild.GetTextChannelAsync(guildData.EmoteSuggestionChannelId.ToUlong());

            if (channel == null)
                throw new ValidationException("Tvůj návrh na emote byl zařazen ke zpracování, ale kvůli technickým důvodům jej nelze nyní zpracovat.");

            if (suggestion.BinaryData == null)
                throw new GrillBotException("Nepodařilo se stáhnout požadovaný emote. Zkus to prosím znovu.");

            using (var ms = new MemoryStream(suggestion.BinaryData))
            {
                var attachment = new FileAttachment(ms, suggestion.BinaryDataFilename);
                await channel.SendFileAsync(attachment, data);
            }
        }
        catch (Exception ex) when (ex is not GrillBotException)
        {
            if (suggestion.Id == default)
            {
                using var context = DbFactory.Create();

                await context.AddAsync(suggestion);
                await context.SaveChangesAsync();
            }

            throw;
        }
    }
}
