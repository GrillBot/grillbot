﻿using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Models.Pagination;
using GrillBot.Data.Models.API.Users;

namespace GrillBot.App.Actions.Api.V1.User;

public class GetUserList : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }
    private ITextsManager Texts { get; }

    public GetUserList(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IDiscordClient discordClient,
        ITextsManager texts) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        DiscordClient = discordClient;
        Texts = texts;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = (GetUserListParams)Parameters[0]!;

        FixStatus(parameters);
        ValidateParameters(parameters);

        using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.User.GetUsersListAsync(parameters, parameters.Pagination);
        var result = await PaginatedResponse<UserListItem>.CopyAndMapAsync(data, MapItemAsync);

        return ApiResult.Ok(result);
    }

    private async Task<UserListItem> MapItemAsync(Database.Entity.User entity)
    {
        var result = new UserListItem
        {
            Username = entity.Username,
            Flags = entity.Flags,
            Guilds = new Dictionary<string, bool>(),
            Id = entity.Id,
            GlobalAlias = entity.GlobalAlias,
            DiscordStatus = entity.Status,
            HaveBirthday = entity.Birthday is not null,
            RegisteredAt = SnowflakeUtils.FromSnowflake(entity.Id.ToUlong()).LocalDateTime
        };

        foreach (var guild in entity.Guilds.OrderBy(o => o.Guild!.Name))
        {
            var discordGuild = await DiscordClient.GetGuildAsync(guild.GuildId.ToUlong());
            var guildUser = discordGuild != null ? await discordGuild.GetUserAsync(guild.UserId.ToUlong()) : null;
            var guildName = !string.IsNullOrEmpty(guild.Nickname) ? $"{guild.Guild!.Name} ({guild.Nickname})" : guild.Guild!.Name;

            result.Guilds.Add(guildName, guildUser != null);
        }

        return result;
    }

    public void FixStatus(GetUserListParams parameters)
    {
        parameters.Status = parameters.Status switch
        {
            UserStatus.Invisible => UserStatus.Offline,
            UserStatus.AFK => UserStatus.Idle,
            _ => parameters.Status
        };
    }

    private void ValidateParameters(GetUserListParams parameters)
    {
        if (parameters.HideLeftUsers && string.IsNullOrEmpty(parameters.GuildId))
        {
            throw new ValidationException(Texts["User/Validation/CannotHideMissingGuildId", ApiContext.Language])
                .ToBadRequestValidation(parameters.HideLeftUsers, nameof(parameters.HideLeftUsers), nameof(parameters.GuildId));
        }
    }
}
