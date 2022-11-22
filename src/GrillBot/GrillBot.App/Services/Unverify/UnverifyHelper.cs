using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Localization;
using GrillBot.Data.Models.API.AuditLog.Filters;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.Unverify;

public class UnverifyHelper
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public UnverifyHelper(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task<IRole> GetMuteRoleAsync(IGuild guild)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var guildData = await repository.Guild.FindGuildAsync(guild, true);
        return string.IsNullOrEmpty(guildData?.MuteRoleId) ? null : guild.GetRole(guildData.MuteRoleId.ToUlong());
    }

    public async Task<string> GetUserLanguageAsync(IGuildUser user, string commandLanguage, bool selfunverify)
    {
        if (selfunverify) return commandLanguage;

        var parameters = new AuditLogListParams
        {
            Sort = { Descending = true, OrderBy = "CreatedBy" },
            Types = new List<AuditLogItemType> { AuditLogItemType.InteractionCommand },
            GuildId = user.GuildId.ToString(),
            ProcessedUserIds = new List<string> { user.Id.ToString() }
        };

        await using var repository = DatabaseBuilder.CreateRepository();

        var logs = await repository.AuditLog.GetSimpleDataAsync(parameters, 1);
        return logs.Count == 0 ? TextsManager.DefaultLocale : JsonConvert.DeserializeObject<InteractionCommandExecuted>(logs[0].Data, AuditLogWriter.SerializerSettings)!.Locale;
    }
}
