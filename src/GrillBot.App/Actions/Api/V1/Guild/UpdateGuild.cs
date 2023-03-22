﻿using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Extensions;
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

        var dbGuild = await repository.Guild.GetOrCreateGuildAsync(guild);
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

        if (!string.IsNullOrEmpty(parameters.BotRoomChannelId) && await guild.GetTextChannelAsync(parameters.BotRoomChannelId.ToUlong()) == null)
            ThrowValidationException("BotRoomChannelNotFound", parameters.BotRoomChannelId, nameof(parameters.BotRoomChannelId));
        else
            dbGuild.BotRoomChannelId = parameters.BotRoomChannelId;

        dbGuild.EmoteSuggestionsFrom = parameters.EmoteSuggestionsValidity?.From;
        dbGuild.EmoteSuggestionsTo = parameters.EmoteSuggestionsValidity?.To;

        await repository.CommitAsync();
        return await GetGuildDetail.ProcessAsync(id);
    }

    private void ThrowValidationException(string errorMessageId, object value, params string[] memberNames)
    {
        throw new ValidationException(new ValidationResult(Texts[$"GuildModule/UpdateGuild/{errorMessageId}", ApiContext.Language], memberNames), null, value);
    }
}
