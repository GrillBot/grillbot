using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API.Guilds;

namespace GrillBot.App.Actions.Api.V1.Guild;

public class UpdateGuild : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private GetGuildDetail GetGuildDetail { get; }
    private ITextsManager Texts { get; }

    public UpdateGuild(ApiRequestContext apiContext, IDiscordClient discordClient, GrillBotDatabaseBuilder databaseBuilder, GetGuildDetail getGuildDetail, ITextsManager texts) : base(apiContext)
    {
        DiscordClient = discordClient;
        DatabaseBuilder = databaseBuilder;
        GetGuildDetail = getGuildDetail;
        Texts = texts;
    }

    public async Task<GuildDetail> ProcessAsync(ulong id, UpdateGuildParams parameters)
    {
        var guild = await DiscordClient.GetGuildAsync(id);
        if (guild == null)
            throw new NotFoundException(Texts["GuildModule/GuildDetail/NotFound", ApiContext.Language]);

        await using var repository = DatabaseBuilder.CreateRepository();

        var dbGuild = await repository.Guild.GetOrCreateRepositoryAsync(guild);
        if (!string.IsNullOrEmpty(parameters.AdminChannelId) && await guild.GetTextChannelAsync(parameters.AdminChannelId.ToUlong()) == null)
            ThrowValidationException("AdminChannelNotFound", parameters.AdminChannelId, nameof(parameters.AdminChannelId));
        else
            dbGuild.AdminChannelId = parameters.AdminChannelId;

        if (!string.IsNullOrEmpty(parameters.MuteRoleId) && guild.GetRole(parameters.MuteRoleId.ToUlong()) == null)
            ThrowValidationException("MuteRoleNotFound", parameters.MuteRoleId, nameof(parameters.MuteRoleId));
        else
            dbGuild.MuteRoleId = parameters.MuteRoleId;

        if (!string.IsNullOrEmpty(parameters.EmoteSuggestionChannelId) && await guild.GetTextChannelAsync(parameters.EmoteSuggestionChannelId.ToUlong()) == null)
            ThrowValidationException("EmoteSuggestionChannelNotFound", parameters.EmoteSuggestionChannelId, nameof(parameters.EmoteSuggestionChannelId));
        else
            dbGuild.EmoteSuggestionChannelId = parameters.EmoteSuggestionChannelId;

        if (!string.IsNullOrEmpty(parameters.VoteChannelId) && await guild.GetTextChannelAsync(parameters.VoteChannelId.ToUlong()) == null)
            ThrowValidationException("VoteChannelNotFound", parameters.VoteChannelId, nameof(parameters.VoteChannelId));
        else
            dbGuild.VoteChannelId = parameters.VoteChannelId;

        UpdateGuildEvents(dbGuild, parameters);

        await repository.CommitAsync();
        return await GetGuildDetail.ProcessAsync(id);
    }

    private void ThrowValidationException(string errorMessageId, object value, params string[] memberNames)
    {
        throw new ValidationException(new ValidationResult(Texts[$"GuildModule/UpdateGuild/{errorMessageId}", ApiContext.Language], memberNames), null, value);
    }

    private static void UpdateGuildEvents(Database.Entity.Guild guild, UpdateGuildParams parameters)
    {
        const string id = "EmoteSuggestions";

        var guildEvent = guild.GuildEvents.FirstOrDefault(o => o.Id == id);
        if (parameters.EmoteSuggestionsValidity == null)
        {
            if (guildEvent != null)
                guild.GuildEvents.Remove(guildEvent);
        }
        else
        {
            if (guildEvent == null)
            {
                guildEvent = new Database.Entity.GuildEvent { Id = id };
                guild.GuildEvents.Add(guildEvent);
            }

            guildEvent.From = parameters.EmoteSuggestionsValidity.From;
            guildEvent.To = parameters.EmoteSuggestionsValidity.To;
        }
    }
}
