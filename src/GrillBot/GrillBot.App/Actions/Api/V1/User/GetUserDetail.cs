using AutoMapper;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Unverify;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.AuditLog.Filters;
using GrillBot.Data.Models.API.Unverify;
using GrillBot.Data.Models.API.Users;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using GrillBot.Database.Enums.Internal;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Actions.Api.V1.User;

public class GetUserDetail : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }
    private IDiscordClient DiscordClient { get; }
    private ITextsManager Texts { get; }

    public GetUserDetail(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper, IDiscordClient discordClient, ITextsManager texts) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        DiscordClient = discordClient;
        Texts = texts;
    }

    public async Task<UserDetail> ProcessSelfAsync()
    {
        var userId = ApiContext.GetUserId();

        var result = await ProcessAsync(userId);
        if (ApiContext.IsPublic())
            result.RemoveSecretData();
        return result;
    }

    public async Task<UserDetail> ProcessAsync(ulong id)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var entity = await repository.User.FindUserByIdAsync(id, UserIncludeOptions.All, true);
        if (entity == null)
            throw new NotFoundException(Texts["User/NotFound", ApiContext.Language]);

        var result = Mapper.Map<UserDetail>(entity);

        result = await UpdateFromDiscordAsync(result, id);
        foreach (var guild in entity.Guilds)
            result.Guilds.Add(await CreateGuildDetailAsync(guild, repository));

        result.Guilds = result.Guilds.OrderByDescending(o => o.IsUserInGuild).ThenBy(o => o.Guild.Name).ToList();
        return result;
    }

    private async Task<UserDetail> UpdateFromDiscordAsync(UserDetail detail, ulong id)
    {
        var discordUser = await DiscordClient.FindUserAsync(id);
        return discordUser == null ? detail : Mapper.Map(discordUser, detail);
    }

    private async Task<GuildUserDetail> CreateGuildDetailAsync(Database.Entity.GuildUser guildUserEntity, GrillBotRepository repository)
    {
        var result = Mapper.Map<GuildUserDetail>(guildUserEntity);

        result.CreatedInvites = result.CreatedInvites.OrderByDescending(o => o.CreatedAt).ToList();
        result.Channels = result.Channels.OrderByDescending(o => o.Count).ThenBy(o => o.Channel.Name).ToList();
        result.Emotes = result.Emotes.OrderByDescending(o => o.UseCount).ThenByDescending(o => o.LastOccurence).ThenBy(o => o.Emote.Name).ToList();

        await UpdateGuildDetailAsync(result, guildUserEntity, repository);
        return result;
    }

    private async Task UpdateGuildDetailAsync(GuildUserDetail detail, Database.Entity.GuildUser entity, GrillBotRepository repository)
    {
        var guild = await DiscordClient.GetGuildAsync(detail.Guild.Id.ToUlong());
        if (guild == null) return;

        detail.IsGuildKnown = true;

        var guildUser = await guild.GetUserAsync(entity.UserId.ToUlong());
        if (guildUser == null) return;

        detail.IsUserInGuild = true;
        SetUnverify(detail, entity.Unverify, guildUser, guild);
        await SetNicknameHistoryAsync(detail, guildUser, repository);
        await SetVisibleChannelsAsync(detail, guildUser, guild);
        detail.Roles = Mapper.Map<List<Role>>(guildUser.GetRoles().OrderByDescending(o => o.Position).ToList());
    }

    private void SetUnverify(GuildUserDetail detail, Database.Entity.Unverify unverify, IGuildUser user, IGuild guild)
    {
        if (unverify == null) return;

        var profile = UnverifyProfileGenerator.Reconstruct(unverify, user, guild);
        detail.Unverify = Mapper.Map<UnverifyInfo>(profile);
    }

    private static async Task SetNicknameHistoryAsync(GuildUserDetail detail, IGuildUser user, GrillBotRepository repository)
    {
        var auditLogParams = new AuditLogListParams
        {
            GuildId = user.GuildId.ToString(),
            Sort = null,
            Types = new List<AuditLogItemType> { AuditLogItemType.MemberUpdated },
            IgnoreBots = true
        };

        var auditLogs = await repository.AuditLog.GetSimpleDataAsync(auditLogParams);
        detail.NicknameHistory = auditLogs
            .Select(o => JsonConvert.DeserializeObject<MemberUpdatedData>(o.Data, AuditLogWriter.SerializerSettings))
            .Where(o => o?.Nickname != null && (!string.IsNullOrEmpty(o.Target.UserId) ? o.Target.UserId : o.Target.Id.ToString()) == user.Id.ToString())
            .SelectMany(o => new[] { o.Nickname.Before, o.Nickname.After })
            .Where(o => !string.IsNullOrEmpty(o) && (string.IsNullOrEmpty(user.Nickname) || user.Nickname != o))
            .Distinct()
            .ToList();
    }

    private async Task SetVisibleChannelsAsync(GuildUserDetail detail, IGuildUser user, IGuild guild)
    {
        var visibleChannels = await guild.GetAvailableChannelsAsync(user);

        detail.VisibleChannels = visibleChannels
            .Where(o => o is not ICategoryChannel)
            .Select(o => Mapper.Map<Data.Models.API.Channels.Channel>(o))
            .OrderBy(o => o.Name)
            .ToList();
    }
}
