using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Extensions;
using GrillBot.Database.Entity;

namespace GrillBot.App.Handlers.GuildMemberUpdated;

public class ServerBoosterHandler(
    GrillBotDatabaseBuilder _databaseBuilder,
    IConfiguration _configuration
) : IGuildMemberUpdatedEvent
{
    public async Task ProcessAsync(IGuildUser? before, IGuildUser after)
    {
        if (!CanProcess(before, after)) return;

        var guild = await FindGuildAsync(after.Guild);
        if (guild is null || string.IsNullOrEmpty(guild.BoosterRoleId) || string.IsNullOrEmpty(guild.AdminChannelId)) return;

        var boostRole = after.Guild.GetRole(guild.BoosterRoleId.ToUlong());
        if (boostRole is null) return;

        var adminChannel = await before!.Guild.GetTextChannelAsync(guild.AdminChannelId.ToUlong());
        if (adminChannel is null) return;

        var boostBefore = before.RoleIds.Any(o => o == boostRole.Id);
        var boostAfter = after.RoleIds.Any(o => o == boostRole.Id);
        if (!IsBoostReallyChanged(boostBefore, boostAfter)) return;

        var embed = new EmbedBuilder()
            .WithColor(boostRole.Color)
            .WithCurrentTimestamp()
            .AddField("Uživatel", $"`{after.GetFullName()}`")
            .WithThumbnailUrl(after.GetUserAvatarUrl());

        switch (boostBefore)
        {
            case false when boostAfter:
                embed.WithTitle($"Uživatel je nyní Server Booster {_configuration["Discord:Emotes:Hypers"]}");
                break;
            case true when !boostAfter:
                embed.WithTitle($"Uživatel již není Server Booster {_configuration["Discord:Emotes:Sadge"]}");
                break;
        }

        await adminChannel.SendMessageAsync(embed: embed.Build());
    }

    private static bool CanProcess(IGuildUser? before, IGuildUser after)
        => before?.RoleIds.SequenceEqual(after.RoleIds) == false;

    private async Task<Guild?> FindGuildAsync(IGuild guild)
    {
        using var repository = _databaseBuilder.CreateRepository();
        return await repository.Guild.FindGuildAsync(guild, true);
    }

    private static bool IsBoostReallyChanged(bool before, bool after)
    {
        var nothingChanged = (before && after) || (!before && !after);
        return !nothingChanged;
    }
}
