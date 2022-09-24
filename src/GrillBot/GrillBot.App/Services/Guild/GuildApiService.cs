using GrillBot.Common.Extensions;
using GrillBot.Data.Models.API.Guilds;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GrillBot.App.Services.Guild;

public class GuildApiService
{
    private IDiscordClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public GuildApiService(GrillBotDatabaseBuilder databaseBuilder, IDiscordClient client)
    {
        DatabaseBuilder = databaseBuilder;
        DiscordClient = client;
    }

    public async Task<GuildDetail> UpdateGuildAsync(ulong id, UpdateGuildParams parameters, ModelStateDictionary modelState)
    {
        var guild = await DiscordClient.GetGuildAsync(id);
        if (guild == null)
            return null;

        await using var repository = DatabaseBuilder.CreateRepository();

        var dbGuild = await repository.Guild.GetOrCreateRepositoryAsync(guild);
        if (!string.IsNullOrEmpty(parameters.AdminChannelId) && await guild.GetTextChannelAsync(parameters.AdminChannelId.ToUlong()) == null)
            modelState.AddModelError(nameof(parameters.AdminChannelId), "Nepodařilo se dohledat administrátorský kanál");
        else
            dbGuild.AdminChannelId = parameters.AdminChannelId;

        if (!string.IsNullOrEmpty(parameters.MuteRoleId) && guild.GetRole(parameters.MuteRoleId.ToUlong()) == null)
            modelState.AddModelError(nameof(parameters.MuteRoleId), "Nepodařilo se dohledat roli, která reprezentuje umlčení uživatele při unverify.");
        else
            dbGuild.MuteRoleId = parameters.MuteRoleId;

        if (!string.IsNullOrEmpty(parameters.EmoteSuggestionChannelId) && await guild.GetTextChannelAsync(parameters.EmoteSuggestionChannelId.ToUlong()) == null)
            modelState.AddModelError(nameof(parameters.EmoteSuggestionChannelId), "Nepodařilo se dohledat kanál pro návrhy emotů.");
        else
            dbGuild.EmoteSuggestionChannelId = parameters.EmoteSuggestionChannelId;

        if (!string.IsNullOrEmpty(parameters.VoteChannelId) && await guild.GetTextChannelAsync(parameters.VoteChannelId.ToUlong()) == null)
            modelState.AddModelError(nameof(parameters.VoteChannelId), "Nepodařilo se dohledat kanál pro veřejná hlasování.");
        else
            dbGuild.VoteChannelId = parameters.VoteChannelId;

        UpdateGuildEvents(dbGuild, parameters);

        if (modelState.IsValid)
            await repository.CommitAsync();
        return null;
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
