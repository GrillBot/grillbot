using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Extensions;
using GrillBot.Database.Entity;

namespace GrillBot.App.Handlers.GuildMemberUpdated;

public class ServerBoosterHandler : IGuildMemberUpdatedEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IConfiguration Configuration { get; }

    public ServerBoosterHandler(GrillBotDatabaseBuilder databaseBuilder, IConfiguration configuration)
    {
        DatabaseBuilder = databaseBuilder;
        Configuration = configuration;
    }

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
                embed.WithTitle($"Uživatel je nyní Server Booster {Configuration["Discord:Emotes:Hypers"]}");
                break;
            case true when !boostAfter:
                embed.WithTitle($"Uživatel již není Server Booster {Configuration["Discord:Emotes:Sadge"]}");
                break;
        }

        await adminChannel.SendMessageAsync(embed: embed.Build());
    }

    private static bool CanProcess(IGuildUser? before, IGuildUser after)
        => before is not null && !before.RoleIds.SequenceEqual(after.RoleIds);

    private async Task<Guild?> FindGuildAsync(IGuild guild)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        return await repository.Guild.FindGuildAsync(guild, true);
    }

    private static bool IsBoostReallyChanged(bool before, bool after)
    {
        var nothingChanged = (before && after) || (!before && !after);
        return !nothingChanged;
    }
}
