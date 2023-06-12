using GrillBot.App.Infrastructure.Jobs;
using GrillBot.App.Managers;
using GrillBot.Core.Models;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Database.Services.Repository;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using NpgsqlTypes;
using Quartz;

// ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract

#pragma warning disable CS8600

namespace GrillBot.App.Jobs;

public class AuditLogMigrationJob : Job
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public AuditLogMigrationJob(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        DatabaseBuilder = serviceProvider.GetRequiredService<GrillBotDatabaseBuilder>();
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        await using var sqlConn = new NpgsqlConnection("Host=nas.misha12.eu;Database=AuditLogService;Username=postgres;Password=*H&b^JUcr?8S&2Ex");
        await sqlConn.OpenAsync();

        await Info(repository, sqlConn);
        await Warning(repository, sqlConn);
        await Error(repository, sqlConn);
        await ChannelCreated(repository, sqlConn);
        await ChannelDeleted(repository, sqlConn);
        await ChannelUpdated(repository, sqlConn);
        await EmojiDeleted(repository, sqlConn);
        await OverwriteCreated(repository, sqlConn);
        await OverwriteDeleted(repository, sqlConn);
        await OverwriteUpdated(repository, sqlConn);
        await Unban(repository, sqlConn);
        await MemberUpdated(repository, sqlConn);
        await MemberRoleUpdated(repository, sqlConn);
        await GuildUpdated(repository, sqlConn);
        await UserLeft(repository, sqlConn);
        await UserJoined(repository, sqlConn);
        await MessageEdited(repository, sqlConn);
        await MessageDeleted(repository, sqlConn);
        await InteractionCommand(repository, sqlConn);
        await ThreadDeleted(repository, sqlConn);
        await JobCompleted(repository, sqlConn);
        await Api(repository, sqlConn);
        await ThreadUpdated(repository, sqlConn);
    }

    private static async Task Info(GrillBotRepository source, NpgsqlConnection target)
    {
        await MigrateAsync<string>(AuditLogItemType.Info, source, target, async (headerId, data) =>
        {
            await using var command = target.CreateCommand();
            command.CommandText = $"INSERT INTO \"LogMessages\" (\"LogItemId\", \"Message\", \"Severity\") VALUES ('{headerId}', '{data}', {(int)LogSeverity.Info})";
            await command.ExecuteNonQueryAsync();
        });
    }

    private static async Task Warning(GrillBotRepository source, NpgsqlConnection target)
    {
        await MigrateAsync<string>(AuditLogItemType.Warning, source, target, async (headerId, data) =>
        {
            await using var command = target.CreateCommand();
            command.CommandText = $"INSERT INTO \"LogMessages\" (\"LogItemId\", \"Message\", \"Severity\") VALUES ('{headerId}', @data, {(int)LogSeverity.Warning})";
            command.Parameters.AddWithValue("@data", NpgsqlDbType.Text, data);

            await command.ExecuteNonQueryAsync();
        });
    }

    private static async Task Error(GrillBotRepository source, NpgsqlConnection target)
    {
        await MigrateAsync<string>(AuditLogItemType.Error, source, target, async (headerId, data) =>
        {
            await using var command = target.CreateCommand();
            command.CommandText = $"INSERT INTO \"LogMessages\" (\"LogItemId\", \"Message\", \"Severity\") VALUES ('{headerId}', @data, {(int)LogSeverity.Error})";
            command.Parameters.AddWithValue("@data", NpgsqlDbType.Text, data);

            await command.ExecuteNonQueryAsync();
        });
    }

    private static async Task ChannelCreated(GrillBotRepository source, NpgsqlConnection target)
    {
        await MigrateAsync<AuditChannelInfo>(AuditLogItemType.ChannelCreated, source, target, async (headerId, data) =>
        {
            var channelInfoId = Guid.NewGuid();
            var channel = (AuditChannelInfo)data;

            await using var channelInfo = target.CreateCommand();
            channelInfo.CommandText = $"INSERT INTO \"ChannelInfoItems\" (\"Id\", \"ChannelName\", \"SlowMode\", \"ChannelType\", \"IsNsfw\", \"Bitrate\", \"Topic\", \"Position\", \"Flags\") " +
                                      $"VALUES ('{channelInfoId}', @name, @slowmode, @type, @nsfw, @bitrate, @topic, @position, @flags)";
            channelInfo.Parameters.AddWithValue("@name", channel.Name);
            channelInfo.Parameters.AddWithValue("@slowmode", (object)channel.SlowMode ?? DBNull.Value);
            channelInfo.Parameters.AddWithValue("@type", (object)(int?)channel.Type ?? DBNull.Value);
            channelInfo.Parameters.AddWithValue("@nsfw", (object)channel.IsNsfw ?? DBNull.Value);
            channelInfo.Parameters.AddWithValue("@bitrate", (object)channel.Bitrate ?? DBNull.Value);
            channelInfo.Parameters.AddWithValue("@topic", (object)channel.Topic ?? DBNull.Value);
            channelInfo.Parameters.AddWithValue("@position", channel.Position);
            channelInfo.Parameters.AddWithValue("@flags", channel.Flags);

            await channelInfo.ExecuteNonQueryAsync();

            await using var channelCreatedItem = target.CreateCommand();
            channelCreatedItem.CommandText = $"INSERT INTO \"ChannelCreatedItems\" (\"LogItemId\", \"ChannelInfoId\") VALUES ('{headerId}', '{channelInfoId}')";
            await channelCreatedItem.ExecuteNonQueryAsync();
        });
    }

    private static async Task ChannelDeleted(GrillBotRepository source, NpgsqlConnection target)
    {
        await MigrateAsync<AuditChannelInfo>(AuditLogItemType.ChannelDeleted, source, target, async (headerId, data) =>
        {
            var channelInfoId = Guid.NewGuid();
            var channel = (AuditChannelInfo)data;

            await using var channelInfo = target.CreateCommand();
            channelInfo.CommandText = $"INSERT INTO \"ChannelInfoItems\" (\"Id\", \"ChannelName\", \"SlowMode\", \"ChannelType\", \"IsNsfw\", \"Bitrate\", \"Topic\", \"Position\", \"Flags\") " +
                                      $"VALUES ('{channelInfoId}', @name, @slowmode, @type, @nsfw, @bitrate, @topic, @position, @flags)";
            channelInfo.Parameters.AddWithValue("@name", channel.Name);
            channelInfo.Parameters.AddWithValue("@slowmode", (object)channel.SlowMode ?? DBNull.Value);
            channelInfo.Parameters.AddWithValue("@type", (object)(int?)channel.Type ?? DBNull.Value);
            channelInfo.Parameters.AddWithValue("@nsfw", (object)channel.IsNsfw ?? DBNull.Value);
            channelInfo.Parameters.AddWithValue("@bitrate", (object)channel.Bitrate ?? DBNull.Value);
            channelInfo.Parameters.AddWithValue("@topic", (object)channel.Topic ?? DBNull.Value);
            channelInfo.Parameters.AddWithValue("@position", channel.Position);
            channelInfo.Parameters.AddWithValue("@flags", channel.Flags);

            await channelInfo.ExecuteNonQueryAsync();

            await using var channelCreatedItem = target.CreateCommand();
            channelCreatedItem.CommandText = $"INSERT INTO \"ChannelDeletedItems\" (\"LogItemId\", \"ChannelInfoId\") VALUES ('{headerId}', '{channelInfoId}')";
            await channelCreatedItem.ExecuteNonQueryAsync();
        });
    }

    private static async Task ChannelUpdated(GrillBotRepository source, NpgsqlConnection target)
    {
        await MigrateAsync<Diff<AuditChannelInfo>>(AuditLogItemType.ChannelUpdated, source, target, async (headerId, data) =>
        {
            var beforeId = Guid.NewGuid();
            var afterId = Guid.NewGuid();
            var diff = (Diff<AuditChannelInfo>)data;

            await using var before = target.CreateCommand();
            before.CommandText = $"INSERT INTO \"ChannelInfoItems\" (\"Id\", \"ChannelName\", \"SlowMode\", \"ChannelType\", \"IsNsfw\", \"Bitrate\", \"Topic\", \"Position\", \"Flags\") " +
                                 $"VALUES ('{beforeId}', @name, @slowmode, @type, @nsfw, @bitrate, @topic, @position, @flags)";
            before.Parameters.AddWithValue("@name", (object)diff.Before.Name ?? DBNull.Value);
            before.Parameters.AddWithValue("@slowmode", (object)diff.Before.SlowMode ?? DBNull.Value);
            before.Parameters.AddWithValue("@type", (object)(int?)diff.Before.Type ?? DBNull.Value);
            before.Parameters.AddWithValue("@nsfw", (object)diff.Before.IsNsfw ?? DBNull.Value);
            before.Parameters.AddWithValue("@bitrate", (object)diff.Before.Bitrate ?? DBNull.Value);
            before.Parameters.AddWithValue("@topic", (object)diff.Before.Topic ?? DBNull.Value);
            before.Parameters.AddWithValue("@position", diff.Before.Position);
            before.Parameters.AddWithValue("@flags", diff.Before.Flags);

            await before.ExecuteNonQueryAsync();

            await using var after = target.CreateCommand();
            after.CommandText = $"INSERT INTO \"ChannelInfoItems\" (\"Id\", \"ChannelName\", \"SlowMode\", \"ChannelType\", \"IsNsfw\", \"Bitrate\", \"Topic\", \"Position\", \"Flags\") " +
                                $"VALUES ('{afterId}', @name, @slowmode, @type, @nsfw, @bitrate, @topic, @position, @flags)";
            after.Parameters.AddWithValue("@name", (object)diff.After.Name ?? DBNull.Value);
            after.Parameters.AddWithValue("@slowmode", (object)diff.After.SlowMode ?? DBNull.Value);
            after.Parameters.AddWithValue("@type", (object)(int?)diff.After.Type ?? DBNull.Value);
            after.Parameters.AddWithValue("@nsfw", (object)diff.After.IsNsfw ?? DBNull.Value);
            after.Parameters.AddWithValue("@bitrate", (object)diff.After.Bitrate ?? DBNull.Value);
            after.Parameters.AddWithValue("@topic", (object)diff.After.Topic ?? DBNull.Value);
            after.Parameters.AddWithValue("@position", diff.After.Position);
            after.Parameters.AddWithValue("@flags", diff.After.Flags);

            await after.ExecuteNonQueryAsync();

            await using var channelCreatedItem = target.CreateCommand();
            channelCreatedItem.CommandText = $"INSERT INTO \"ChannelUpdatedItems\" (\"LogItemId\", \"BeforeId\", \"AfterId\") VALUES ('{headerId}', '{beforeId}', '{afterId}')";
            await channelCreatedItem.ExecuteNonQueryAsync();
        });
    }

    private static async Task EmojiDeleted(GrillBotRepository source, NpgsqlConnection target)
    {
        await MigrateAsync<AuditEmoteInfo>(AuditLogItemType.EmojiDeleted, source, target, async (headerId, data) =>
        {
            var emote = (AuditEmoteInfo)data;
            var emoteId = emote.Id > 0 ? emote.Id.ToString() : emote.EmoteId;

            await using var command = target.CreateCommand();
            command.CommandText = $"INSERT INTO \"DeletedEmotes\" (\"LogItemId\", \"EmoteId\", \"EmoteName\") VALUES ('{headerId}', '{emoteId}', @name)";
            command.Parameters.AddWithValue("@name", emote.Name);
            await command.ExecuteNonQueryAsync();
        });
    }

    private static async Task OverwriteCreated(GrillBotRepository source, NpgsqlConnection target)
    {
        await MigrateAsync<AuditOverwriteInfo>(AuditLogItemType.OverwriteCreated, source, target, async (headerId, data) =>
        {
            var overwriteInfoId = Guid.NewGuid();
            var overwrite = (AuditOverwriteInfo)data;

            await using var infoCommnd = target.CreateCommand();
            infoCommnd.CommandText = $"INSERT INTO \"OverwriteInfoItems\" (\"Id\", \"Target\", \"TargetId\", \"AllowValue\", \"DenyValue\") " +
                                     $"VALUES ('{overwriteInfoId}', @target, @targetId, @allowValue, @denyValue)";
            infoCommnd.Parameters.AddWithValue("@target", (int)overwrite.TargetType);
            infoCommnd.Parameters.AddWithValue("@targetId", overwrite.TargetId > 0 ? overwrite.TargetId.ToString() : overwrite.TargetIdValue);
            infoCommnd.Parameters.AddWithValue("@allowValue", overwrite.AllowValue.ToString());
            infoCommnd.Parameters.AddWithValue("@denyValue", overwrite.DenyValue.ToString());

            await infoCommnd.ExecuteNonQueryAsync();

            await using var channelCreatedItem = target.CreateCommand();
            channelCreatedItem.CommandText = $"INSERT INTO \"OverwriteCreatedItems\" (\"LogItemId\", \"OverwriteInfoId\") VALUES ('{headerId}', '{overwriteInfoId}')";
            await channelCreatedItem.ExecuteNonQueryAsync();
        });
    }

    private static async Task OverwriteDeleted(GrillBotRepository source, NpgsqlConnection target)
    {
        await MigrateAsync<AuditOverwriteInfo>(AuditLogItemType.OverwriteDeleted, source, target, async (headerId, data) =>
        {
            var overwriteInfoId = Guid.NewGuid();
            var overwrite = (AuditOverwriteInfo)data;

            await using var infoCommnd = target.CreateCommand();
            infoCommnd.CommandText = $"INSERT INTO \"OverwriteInfoItems\" (\"Id\", \"Target\", \"TargetId\", \"AllowValue\", \"DenyValue\") " +
                                     $"VALUES ('{overwriteInfoId}', @target, @targetId, @allowValue, @denyValue)";
            infoCommnd.Parameters.AddWithValue("@target", (int)overwrite.TargetType);
            infoCommnd.Parameters.AddWithValue("@targetId", overwrite.TargetId > 0 ? overwrite.TargetId.ToString() : overwrite.TargetIdValue);
            infoCommnd.Parameters.AddWithValue("@allowValue", overwrite.AllowValue.ToString());
            infoCommnd.Parameters.AddWithValue("@denyValue", overwrite.DenyValue.ToString());

            await infoCommnd.ExecuteNonQueryAsync();

            await using var channelCreatedItem = target.CreateCommand();
            channelCreatedItem.CommandText = $"INSERT INTO \"OverwriteDeletedItems\" (\"LogItemId\", \"OverwriteInfoId\") VALUES ('{headerId}', '{overwriteInfoId}')";
            await channelCreatedItem.ExecuteNonQueryAsync();
        });
    }

    private static async Task OverwriteUpdated(GrillBotRepository source, NpgsqlConnection target)
    {
        await MigrateAsync<Diff<AuditOverwriteInfo>>(AuditLogItemType.OverwriteUpdated, source, target, async (headerId, data) =>
        {
            var beforeId = Guid.NewGuid();
            var afterId = Guid.NewGuid();
            var diff = (Diff<AuditOverwriteInfo>)data;

            await using var before = target.CreateCommand();
            before.CommandText = $"INSERT INTO \"OverwriteInfoItems\" (\"Id\", \"Target\", \"TargetId\", \"AllowValue\", \"DenyValue\") " +
                                 $"VALUES ('{beforeId}', @target, @targetId, @allowValue, @denyValue)";
            before.Parameters.AddWithValue("@target", (int)diff.Before.TargetType);
            before.Parameters.AddWithValue("@targetId", diff.Before.TargetId > 0 ? diff.Before.TargetId.ToString() : diff.Before.TargetIdValue);
            before.Parameters.AddWithValue("@allowValue", diff.Before.AllowValue.ToString());
            before.Parameters.AddWithValue("@denyValue", diff.Before.DenyValue.ToString());

            await before.ExecuteNonQueryAsync();

            await using var after = target.CreateCommand();
            after.CommandText = $"INSERT INTO \"OverwriteInfoItems\" (\"Id\", \"Target\", \"TargetId\", \"AllowValue\", \"DenyValue\") " +
                                $"VALUES ('{afterId}', @target, @targetId, @allowValue, @denyValue)";
            after.Parameters.AddWithValue("@target", (int)diff.After.TargetType);
            after.Parameters.AddWithValue("@targetId", diff.After.TargetId > 0 ? diff.After.TargetId.ToString() : diff.After.TargetIdValue);
            after.Parameters.AddWithValue("@allowValue", diff.After.AllowValue.ToString());
            after.Parameters.AddWithValue("@denyValue", diff.After.DenyValue.ToString());

            await after.ExecuteNonQueryAsync();

            await using var channelCreatedItem = target.CreateCommand();
            channelCreatedItem.CommandText = $"INSERT INTO \"OverwriteUpdatedItems\" (\"LogItemId\", \"BeforeId\", \"AfterId\") VALUES ('{headerId}', '{beforeId}', '{afterId}')";
            await channelCreatedItem.ExecuteNonQueryAsync();
        });
    }

    private static async Task Unban(GrillBotRepository source, NpgsqlConnection target)
    {
        await MigrateAsync<AuditUserInfo>(AuditLogItemType.Unban, source, target, async (headerId, data) =>
        {
            var user = (AuditUserInfo)data;
            var id = user.Id > 0 ? user.Id.ToString() : user.UserId;

            await using var channelCreatedItem = target.CreateCommand();
            channelCreatedItem.CommandText = $"INSERT INTO \"Unbans\" (\"LogItemId\", \"UserId\") VALUES ('{headerId}', '{id}')";
            await channelCreatedItem.ExecuteNonQueryAsync();
        });
    }

    private static async Task MemberUpdated(GrillBotRepository source, NpgsqlConnection target)
    {
        await MigrateAsync<MemberUpdatedData>(AuditLogItemType.MemberUpdated, source, target, async (headerId, data) =>
        {
            var member = (MemberUpdatedData)data;
            var userId = member.Target.Id > 0 ? member.Target.Id.ToString() : member.Target.UserId;
            var beforeId = Guid.NewGuid();
            var afterId = Guid.NewGuid();

            await using var before = target.CreateCommand();
            before.CommandText = $"INSERT INTO \"MemberInfos\" (\"Id\", \"UserId\", \"Nickname\", \"IsMuted\", \"IsDeaf\", \"SelfUnverifyMinimalTime\", \"Flags\") " +
                                 $"VALUES ('{beforeId}', '{userId}', @nickname, @muted, @deaf, @selfunverify, @flags)";
            before.Parameters.AddWithValue("@nickname", (object)member.Nickname?.Before ?? DBNull.Value);
            before.Parameters.AddWithValue("@muted", (object)member.IsMuted?.Before ?? DBNull.Value);
            before.Parameters.AddWithValue("@deaf", (object)member.IsDeaf?.Before ?? DBNull.Value);
            before.Parameters.AddWithValue("@selfunverify", (object)member.SelfUnverifyMinimalTime?.Before?.ToString("c") ?? DBNull.Value);
            before.Parameters.AddWithValue("@flags", (object)member.Flags?.Before ?? DBNull.Value);
            await before.ExecuteNonQueryAsync();

            await using var after = target.CreateCommand();
            after.CommandText = $"INSERT INTO \"MemberInfos\" (\"Id\", \"UserId\", \"Nickname\", \"IsMuted\", \"IsDeaf\", \"SelfUnverifyMinimalTime\", \"Flags\") " +
                                 $"VALUES ('{afterId}', '{userId}', @nickname, @muted, @deaf, @selfunverify, @flags)";
            after.Parameters.AddWithValue("@nickname", (object)member.Nickname?.After ?? DBNull.Value);
            after.Parameters.AddWithValue("@muted", (object)member.IsMuted?.After ?? DBNull.Value);
            after.Parameters.AddWithValue("@deaf", (object)member.IsDeaf?.After ?? DBNull.Value);
            after.Parameters.AddWithValue("@selfunverify", (object)member.SelfUnverifyMinimalTime?.After?.ToString("c") ?? DBNull.Value);
            after.Parameters.AddWithValue("@flags", (object)member.Flags?.After ?? DBNull.Value);
            await after.ExecuteNonQueryAsync();

            await using var channelCreatedItem = target.CreateCommand();
            channelCreatedItem.CommandText = $"INSERT INTO \"MemberUpdatedItems\" (\"LogItemId\", \"BeforeId\", \"AfterId\") VALUES ('{headerId}', '{beforeId}', '{afterId}')";
            await channelCreatedItem.ExecuteNonQueryAsync();
        });
    }

    private static async Task MemberRoleUpdated(GrillBotRepository source, NpgsqlConnection target)
    {
        await MigrateAsync<MemberUpdatedData>(AuditLogItemType.MemberRoleUpdated, source, target, async (headerId, data) =>
        {
            var member = (MemberUpdatedData)data;
            var userId = member.Target.Id > 0 ? member.Target.Id.ToString() : member.Target.UserId;

            foreach (var role in member.Roles ?? new List<AuditRoleUpdateInfo>())
            {
                var id = Guid.NewGuid();
                var roleId = role.Id > 0 ? role.Id.ToString() : role.RoleId;

                await using var roleCommand = target.CreateCommand();
                roleCommand.CommandText = $"INSERT INTO \"MemberRoleUpdatedItems\" (\"Id\", \"LogItemId\", \"UserId\", \"RoleId\", \"RoleName\", \"RoleColor\", \"IsAdded\") " +
                                     $"VALUES ('{id}', '{headerId}', '{userId}', '{roleId}', @name, @color, @added)";
                roleCommand.Parameters.AddWithValue("@name", role.Name);
                roleCommand.Parameters.AddWithValue("@color", new Color(role.Color).ToString());
                roleCommand.Parameters.AddWithValue("@added", role.Added);
                await roleCommand.ExecuteNonQueryAsync();
            }
        });
    }

    private static async Task GuildUpdated(GrillBotRepository source, NpgsqlConnection target)
    {
        await MigrateAsync<GuildUpdatedData>(AuditLogItemType.GuildUpdated, source, target, async (headerId, data) =>
        {
            var beforeId = Guid.NewGuid();
            var afterId = Guid.NewGuid();
            var guild = (GuildUpdatedData)data;

            await using var before = target.CreateCommand();
            before.CommandText =
                $"INSERT INTO \"GuildInfoItems\" (\"Id\", \"DefaultMessageNotifications\", \"Description\", \"VanityUrl\", \"BannerId\", \"DiscoverySplashId\", \"SplashId\", \"IconId\", \"IconData\", \"PublicUpdatesChannelId\", \"SystemChannelId\", \"AfkChannelId\", \"AfkTimeout\", \"Name\", \"MfaLevel\", \"VerificationLevel\", \"ExplicitContentFilter\", \"Features\", \"PremiumTier\", \"SystemChannelFlags\", \"NsfwLevel\") " +
                $"VALUES ('{beforeId}', @defaultMessageNotifications, @description, @vanityUrl, @bannerId, @discoverySplashId, @splashId, @iconId, NULL, @publicUpdatesChannelId, @systemChannelId, @afkChannelId, @afkTimeout, @name, @mfaLevel, @verificationLevel, @explicitContentFilter, @features, @premiumTier, @systemChannelFlags, @nsfwLevel)";
            before.Parameters.AddWithValue("@defaultMessageNotifications", (int?)guild.DefaultMessageNotifications?.Before ?? 1);
            before.Parameters.AddWithValue("@description", (object)guild.Description?.Before ?? DBNull.Value);
            before.Parameters.AddWithValue("@vanityUrl", (object)guild.VanityUrl?.Before ?? DBNull.Value);
            before.Parameters.AddWithValue("@bannerId", guild.BannerChanged ? "Changed" : DBNull.Value);
            before.Parameters.AddWithValue("@discoverySplashId", guild.DiscoverySplashChanged ? "Changed" : DBNull.Value);
            before.Parameters.AddWithValue("@splashId", guild.SplashChanged ? "Changed" : DBNull.Value);
            before.Parameters.AddWithValue("@iconId", guild.IconChanged ? "Changed" : DBNull.Value);
            before.Parameters.AddWithValue("@publicUpdatesChannelId", (object)guild.PublicUpdatesChannel?.Before?.GetId() ?? DBNull.Value);
            before.Parameters.AddWithValue("@systemChannelId", (object)guild.SystemChannel?.Before?.GetId() ?? DBNull.Value);
            before.Parameters.AddWithValue("@afkChannelId", (object)guild.AfkChannel?.Before?.GetId() ?? DBNull.Value);
            before.Parameters.AddWithValue("@afkTimeout", (object)guild.AfkTimeout?.Before ?? 0);
            before.Parameters.AddWithValue("@name", (object)guild.Name?.Before ?? "-");
            before.Parameters.AddWithValue("@mfaLevel", (int)(guild.MfaLevel?.Before ?? MfaLevel.Enabled));
            before.Parameters.AddWithValue("@verificationLevel", (int)(guild.VerificationLevel?.Before ?? VerificationLevel.Medium));
            before.Parameters.AddWithValue("@explicitContentFilter", (int)(guild.ExplicitContentFilter?.Before ?? ExplicitContentFilterLevel.AllMembers));
            before.Parameters.AddWithValue("@features", (int)(guild.Features?.Before ?? GuildFeature.None));
            before.Parameters.AddWithValue("@premiumTier", (int)(guild.PremiumTier?.Before ?? PremiumTier.None));
            before.Parameters.AddWithValue("@systemChannelFlags", (int)(guild.SystemChannelFlags?.Before ?? SystemChannelMessageDeny.None));
            before.Parameters.AddWithValue("@nsfwLevel", (int)(guild.NsfwLevel?.Before ?? NsfwLevel.Default));
            await before.ExecuteNonQueryAsync();

            await using var after = target.CreateCommand();
            after.CommandText =
                $"INSERT INTO \"GuildInfoItems\" (\"Id\", \"DefaultMessageNotifications\", \"Description\", \"VanityUrl\", \"BannerId\", \"DiscoverySplashId\", \"SplashId\", \"IconId\", \"IconData\", \"PublicUpdatesChannelId\", \"SystemChannelId\", \"AfkChannelId\", \"AfkTimeout\", \"Name\", \"MfaLevel\", \"VerificationLevel\", \"ExplicitContentFilter\", \"Features\", \"PremiumTier\", \"SystemChannelFlags\", \"NsfwLevel\") " +
                $"VALUES ('{afterId}', @defaultMessageNotifications, @description, @vanityUrl, @bannerId, @discoverySplashId, @splashId, @iconId, NULL, @publicUpdatesChannelId, @systemChannelId, @afkChannelId, @afkTimeout, @name, @mfaLevel, @verificationLevel, @explicitContentFilter, @features, @premiumTier, @systemChannelFlags, @nsfwLevel)";
            after.Parameters.AddWithValue("@defaultMessageNotifications", (int?)guild.DefaultMessageNotifications?.After ?? 1);
            after.Parameters.AddWithValue("@description", (object)guild.Description?.After ?? DBNull.Value);
            after.Parameters.AddWithValue("@vanityUrl", (object)guild.VanityUrl?.After ?? DBNull.Value);
            after.Parameters.AddWithValue("@bannerId", guild.BannerChanged ? "Changed" : DBNull.Value);
            after.Parameters.AddWithValue("@discoverySplashId", guild.DiscoverySplashChanged ? "Changed" : DBNull.Value);
            after.Parameters.AddWithValue("@splashId", guild.SplashChanged ? "Changed" : DBNull.Value);
            after.Parameters.AddWithValue("@iconId", guild.IconChanged ? "Changed" : DBNull.Value);
            after.Parameters.AddWithValue("@publicUpdatesChannelId", (object)guild.PublicUpdatesChannel?.After?.GetId() ?? DBNull.Value);
            after.Parameters.AddWithValue("@systemChannelId", (object)guild.SystemChannel?.After?.GetId() ?? DBNull.Value);
            after.Parameters.AddWithValue("@afkChannelId", (object)guild.AfkChannel?.After?.GetId() ?? DBNull.Value);
            after.Parameters.AddWithValue("@afkTimeout", (object)guild.AfkTimeout?.After ?? 0);
            after.Parameters.AddWithValue("@name", (object)guild.Name?.After ?? "-");
            after.Parameters.AddWithValue("@mfaLevel", (int)(guild.MfaLevel?.After ?? MfaLevel.Enabled));
            after.Parameters.AddWithValue("@verificationLevel", (int)(guild.VerificationLevel?.After ?? VerificationLevel.Medium));
            after.Parameters.AddWithValue("@explicitContentFilter", (int)(guild.ExplicitContentFilter?.After ?? ExplicitContentFilterLevel.AllMembers));
            after.Parameters.AddWithValue("@features", (int)(guild.Features?.After ?? GuildFeature.None));
            after.Parameters.AddWithValue("@premiumTier", (int)(guild.PremiumTier?.After ?? PremiumTier.None));
            after.Parameters.AddWithValue("@systemChannelFlags", (int)(guild.SystemChannelFlags?.After ?? SystemChannelMessageDeny.None));
            after.Parameters.AddWithValue("@nsfwLevel", (int)(guild.NsfwLevel?.After ?? NsfwLevel.Default));
            await after.ExecuteNonQueryAsync();

            await using var changeItem = target.CreateCommand();
            changeItem.CommandText = $"INSERT INTO \"GuildUpdatedItems\" (\"LogItemId\", \"BeforeId\", \"AfterId\") VALUES ('{headerId}', '{beforeId}', '{afterId}')";
            await changeItem.ExecuteNonQueryAsync();
        });
    }

    private static async Task UserLeft(GrillBotRepository source, NpgsqlConnection target)
    {
        await MigrateAsync<UserLeftGuildData>(AuditLogItemType.UserLeft, source, target, async (headerId, data) =>
        {
            var member = (UserLeftGuildData)data;
            var userId = member.User.Id > 0 ? member.User.Id.ToString() : member.User.UserId;

            await using var before = target.CreateCommand();
            before.CommandText = $"INSERT INTO \"UserLeftItems\" (\"LogItemId\", \"UserId\", \"MemberCount\", \"IsBan\", \"BanReason\") " +
                                 $"VALUES ('{headerId}', '{userId}', '{member.MemberCount}', {(member.IsBan ? "true" : "false")}, @reason)";
            before.Parameters.AddWithValue("@reason", (object)member.BanReason ?? DBNull.Value);
            await before.ExecuteNonQueryAsync();
        });
    }

    private static async Task UserJoined(GrillBotRepository source, NpgsqlConnection target)
    {
        await MigrateAsync<UserJoinedAuditData>(AuditLogItemType.UserJoined, source, target, async (headerId, data) =>
        {
            var member = (UserJoinedAuditData)data;

            await using var before = target.CreateCommand();
            before.CommandText = $"INSERT INTO \"UserJoinedItems\" (\"LogItemId\", \"MemberCount\") VALUES ('{headerId}', '{member.MemberCount}')";
            await before.ExecuteNonQueryAsync();
        });
    }

    private static async Task MessageEdited(GrillBotRepository source, NpgsqlConnection target)
    {
        await MigrateAsync<MessageEditedData>(AuditLogItemType.MessageEdited, source, target, async (headerId, data) =>
        {
            var msg = (MessageEditedData)data;

            await using var before = target.CreateCommand();
            before.CommandText = $"INSERT INTO \"MessageEditedItems\" (\"LogItemId\", \"JumpUrl\", \"ContentBefore\", \"ContentAfter\") VALUES ('{headerId}', @jumpUrl, @before, @after)";
            before.Parameters.AddWithValue("@jumpUrl", msg.JumpUrl);
            before.Parameters.AddWithValue("@before", msg.Diff.Before);
            before.Parameters.AddWithValue("@after", msg.Diff.After);
            await before.ExecuteNonQueryAsync();
        });
    }

    private static async Task MessageDeleted(GrillBotRepository source, NpgsqlConnection target)
    {
        await MigrateAsync<MessageDeletedData>(AuditLogItemType.MessageDeleted, source, target, async (headerId, data) =>
        {
            var msg = (MessageDeletedData)data;
            var authorId = msg.Data.Author.GetId();
            var msgCreated = msg.Data.CreatedAt;
            var messageCreatedAt = new DateTime(msgCreated.Year, msgCreated.Month, msgCreated.Day, msgCreated.Hour, msgCreated.Minute, msgCreated.Second, msgCreated.Millisecond, DateTimeKind.Local)
                .ToUniversalTime().ToString("o");

            await using var message = target.CreateCommand();
            message.CommandText =
                $"INSERT INTO \"MessageDeletedItems\" (\"LogItemId\", \"AuthorId\", \"MessageCreatedAt\", \"Content\") VALUES ('{headerId}', '{authorId}', '{messageCreatedAt}', @content)";
            message.Parameters.AddWithValue("@content", msg.Data.Content ?? "");
            await message.ExecuteNonQueryAsync();

            if (msg.Data.Embeds is null)
                return;

            foreach (var embed in msg.Data.Embeds)
            {
                var embedId = Guid.NewGuid();

                await using var embedInfo = target.CreateCommand();
                embedInfo.CommandText =
                    $"INSERT INTO \"EmbedInfoItems\" (\"Id\", \"MessageDeletedId\", \"Title\", \"Type\", \"ImageInfo\", \"VideoInfo\", \"AuthorName\", \"ContainsFooter\", \"ProviderName\", \"ThumbnailInfo\") " +
                    $"VALUES ('{embedId}', '{headerId}', @title, '{embed.Type}', @imageInfo, @videoInfo, @authorName, {embed.ContainsFooter.ToString().ToLower()}, @providerName, @thumbnailInfo)";
                embedInfo.Parameters.AddWithValue("@title", (object)embed.Title ?? DBNull.Value);
                embedInfo.Parameters.AddWithValue("@imageInfo", (object)embed.ImageInfo ?? DBNull.Value);
                embedInfo.Parameters.AddWithValue("@videoInfo", (object)embed.VideoInfo ?? DBNull.Value);
                embedInfo.Parameters.AddWithValue("@authorName", (object)embed.AuthorName ?? DBNull.Value);
                embedInfo.Parameters.AddWithValue("@providerName", (object)embed.ProviderName ?? DBNull.Value);
                embedInfo.Parameters.AddWithValue("@thumbnailInfo", (object)embed.ThumbnailInfo ?? DBNull.Value);
                await embedInfo.ExecuteNonQueryAsync();

                if (embed.Fields is null)
                    continue;

                foreach (var field in embed.Fields)
                {
                    await using var embedField = target.CreateCommand();
                    embedField.CommandText =
                        $"INSERT INTO \"EmbedFields\" (\"Id\", \"EmbedInfoId\", \"Name\", \"Value\", \"Inline\") VALUES ('{Guid.NewGuid()}', '{embedId}', @name, @value, '{field.Inline.ToString().ToLower()}')";
                    embedField.Parameters.AddWithValue("@name", field.Name);
                    embedField.Parameters.AddWithValue("@value", field.Value);
                    await embedField.ExecuteNonQueryAsync();
                }
            }
        });
    }

    private static async Task InteractionCommand(GrillBotRepository source, NpgsqlConnection target)
    {
        await MigrateAsync<InteractionCommandExecuted>(AuditLogItemType.InteractionCommand, source, target, async (headerId, data) =>
        {
            var cmd = (InteractionCommandExecuted)data;

            var responded = cmd.HasResponded.ToString().ToLower();
            var validToken = cmd.IsValidToken.ToString().ToLower();
            var success = cmd.IsSuccess.ToString().ToLower();
            var commandError = cmd.CommandError is null ? "NULL" : ((int)cmd.CommandError.Value).ToString();

            await using var command = target.CreateCommand();
            command.CommandText =
                "INSERT INTO \"InteractionCommands\" (\"LogItemId\", \"Name\", \"ModuleName\", \"MethodName\", \"Parameters\", \"HasResponded\", \"IsValidToken\", \"IsSuccess\", \"CommandError\", \"ErrorReason\", \"Duration\", \"Exception\", \"Locale\") " +
                $"VALUES ('{headerId}', '{cmd.Name}', '{cmd.ModuleName}', '{cmd.MethodName}', @parameters, {responded}, {validToken}, {success}, {commandError}, @errorReason, {cmd.Duration}, @exception, '{cmd.Locale}')";

            var parameters = new JArray();
            if (cmd.Parameters is not null)
            {
                foreach (var param in cmd.Parameters)
                {
                    parameters.Add(new JObject
                    {
                        { "Name", param.Name },
                        { "Type", param.Type },
                        { "Value", param.Value?.ToString() }
                    });
                }
            }

            command.Parameters.AddWithValue("@parameters", NpgsqlDbType.Jsonb, parameters.ToString(Formatting.None));
            command.Parameters.AddWithValue("@errorReason", (object)cmd.ErrorReason ?? DBNull.Value);
            command.Parameters.AddWithValue("@exception", (object)cmd.Exception ?? DBNull.Value);
            await command.ExecuteNonQueryAsync();
        });
    }

    private static async Task ThreadDeleted(GrillBotRepository source, NpgsqlConnection target)
    {
        await MigrateAsync<AuditThreadInfo>(AuditLogItemType.ThreadDeleted, source, target, async (headerId, data) =>
        {
            var threadInfoId = Guid.NewGuid();
            var thread = (AuditThreadInfo)data;

            var slowMode = thread.SlowMode is null ? "NULL" : thread.SlowMode.Value.ToString();

            await using var threadInfo = target.CreateCommand();
            threadInfo.CommandText = $"INSERT INTO \"ThreadInfoItems\" (\"Id\", \"ThreadName\", \"SlowMode\", \"Type\", \"IsArchived\", \"ArchiveDuration\", \"IsLocked\", \"Tags\") " +
                                     $"VALUES ('{threadInfoId}', @name, {slowMode}, {(int)thread.ThreadType}, {thread.IsArchived.ToString().ToLower()}, {(int)thread.ArchiveDuration}, {thread.IsLocked.ToString().ToLower()}, @tags)";
            threadInfo.Parameters.AddWithValue("@name", thread.Name);
            threadInfo.Parameters.AddWithValue("@tags", NpgsqlDbType.Jsonb, thread.Tags ?? new List<string>());
            await threadInfo.ExecuteNonQueryAsync();

            await using var threadDeletedItem = target.CreateCommand();
            threadDeletedItem.CommandText = $"INSERT INTO \"ThreadDeletedItems\" (\"LogItemId\", \"ThreadInfoId\") VALUES ('{headerId}', '{threadInfoId}')";
            await threadDeletedItem.ExecuteNonQueryAsync();
        });
    }

    private static async Task JobCompleted(GrillBotRepository source, NpgsqlConnection target)
    {
        await MigrateAsync<JobExecutionData>(AuditLogItemType.JobCompleted, source, target, async (headerId, data) =>
        {
            var job = (JobExecutionData)data;
            var start = new DateTime(job.StartAt.Year, job.StartAt.Month, job.StartAt.Day, job.StartAt.Hour, job.StartAt.Minute, job.StartAt.Second, job.StartAt.Millisecond, DateTimeKind.Local)
                .ToUniversalTime().ToString("o");
            var end = new DateTime(job.EndAt.Year, job.EndAt.Month, job.EndAt.Day, job.EndAt.Hour, job.EndAt.Minute, job.EndAt.Second, job.EndAt.Millisecond, DateTimeKind.Local).ToUniversalTime()
                .ToString("o");
            var startUser = job.StartingUser is null ? "NULL" : $"'{job.StartingUser.GetId()}'";

            await using var jobInfo = target.CreateCommand();
            jobInfo.CommandText = $"INSERT INTO \"JobExecutions\" (\"LogItemId\", \"JobName\", \"Result\", \"StartAt\", \"EndAt\", \"WasError\", \"StartUserId\") " +
                                  $"VALUES ('{headerId}', '{job.JobName}', @result, '{start}', '{end}', {job.WasError.ToString().ToLower()}, {startUser})";
            jobInfo.Parameters.AddWithValue("@result", job.Result!);
            await jobInfo.ExecuteNonQueryAsync();
        });
    }

    private static async Task Api(GrillBotRepository source, NpgsqlConnection target)
    {
        await MigrateAsync<ApiRequest>(AuditLogItemType.Api, source, target, async (headerId, data) =>
        {
            var api = (ApiRequest)data;
            var start = new DateTime(api.StartAt.Year, api.StartAt.Month, api.StartAt.Day, api.StartAt.Hour, api.StartAt.Minute, api.StartAt.Second, api.StartAt.Millisecond, DateTimeKind.Local)
                .ToUniversalTime().ToString("o");
            var end = new DateTime(api.EndAt.Year, api.EndAt.Month, api.EndAt.Day, api.EndAt.Hour, api.EndAt.Minute, api.EndAt.Second, api.EndAt.Millisecond, DateTimeKind.Local).ToUniversalTime()
                .ToString("o");

            await using var request = target.CreateCommand();
            request.CommandText =
                $"INSERT INTO \"ApiRequests\" (\"LogItemId\", \"ControllerName\", \"ActionName\", \"StartAt\", \"EndAt\", \"Method\", \"TemplatePath\", \"Path\", \"Parameters\", \"Language\", \"ApiGroupName\", \"Headers\", \"Identification\", \"Ip\", \"Result\") " +
                $"VALUES ('{headerId}', '{api.ControllerName}', '{api.ActionName}', '{start}', '{end}', '{api.Method}', @templatePath, @path, @parameters, '{api.Language}', '{(api.ApiGroupName ?? "V1")}', @headers, @identification, '{api.IpAddress}', '{api.StatusCode}')";
            request.Parameters.AddWithValue("@templatePath", api.TemplatePath);
            request.Parameters.AddWithValue("@path", api.Path);
            request.Parameters.AddWithValue("@parameters", NpgsqlDbType.Jsonb, api.Parameters);
            request.Parameters.AddWithValue("@headers", NpgsqlDbType.Jsonb, api.Headers);
            request.Parameters.AddWithValue("@identification", api.UserIdentification ?? "UnknownIdentification");
            await request.ExecuteNonQueryAsync();
        });
    }

    private static async Task ThreadUpdated(GrillBotRepository source, NpgsqlConnection target)
    {
        await MigrateAsync<Diff<AuditThreadInfo>>(AuditLogItemType.ThreadUpdated, source, target, async (headerId, data) =>
        {
            var beforeId = Guid.NewGuid();
            var afterId = Guid.NewGuid();
            var diff = (Diff<AuditThreadInfo>)data;

            await using var before = target.CreateCommand();
            before.CommandText = $"INSERT INTO \"ThreadInfoItems\" (\"Id\", \"ThreadName\", \"SlowMode\", \"Type\", \"IsArchived\", \"ArchiveDuration\", \"IsLocked\", \"Tags\") " +
                                 $"VALUES ('{beforeId}', @name, @slowmode, @type, '{diff.Before.IsArchived.ToString().ToLower()}', {(int)diff.Before.ArchiveDuration}, {diff.Before.IsLocked.ToString().ToLower()}, @tags)";
            before.Parameters.AddWithValue("@name", (object)diff.Before.Name ?? DBNull.Value);
            before.Parameters.AddWithValue("@slowmode", (object)diff.Before.SlowMode ?? DBNull.Value);
            before.Parameters.AddWithValue("@type", (int)diff.Before.ThreadType);
            before.Parameters.AddWithValue("@tags", NpgsqlDbType.Jsonb, diff.Before.Tags ?? new List<string>());
            await before.ExecuteNonQueryAsync();

            await using var after = target.CreateCommand();
            after.CommandText = $"INSERT INTO \"ThreadInfoItems\" (\"Id\", \"ThreadName\", \"SlowMode\", \"Type\", \"IsArchived\", \"ArchiveDuration\", \"IsLocked\", \"Tags\") " +
                                $"VALUES ('{afterId}', @name, @slowmode, @type, '{diff.After.IsArchived.ToString().ToLower()}', {(int)diff.After.ArchiveDuration}, {diff.After.IsLocked.ToString().ToLower()}, @tags)";
            after.Parameters.AddWithValue("@name", (object)diff.After.Name ?? DBNull.Value);
            after.Parameters.AddWithValue("@slowmode", (object)diff.After.SlowMode ?? DBNull.Value);
            after.Parameters.AddWithValue("@type", (int)diff.After.ThreadType);
            after.Parameters.AddWithValue("@tags", NpgsqlDbType.Jsonb, diff.After.Tags ?? new List<string>());
            await after.ExecuteNonQueryAsync();

            await using var changeItem = target.CreateCommand();
            changeItem.CommandText = $"INSERT INTO \"ThreadUpdatedItems\" (\"LogItemId\", \"BeforeId\", \"AfterId\") VALUES ('{headerId}', '{beforeId}', '{afterId}')";
            await changeItem.ExecuteNonQueryAsync();
        });
    }

    private static async Task MigrateAsync<TSourceType>(AuditLogItemType type, GrillBotRepository source, NpgsqlConnection target, Func<Guid, object, Task> insertItem)
    {
        var rawItems = await source.AuditLog.GetItemsByType(type);
        var saveCount = 0;

        foreach (var item in rawItems)
        {
            if (item.Type == AuditLogItemType.Api)
            {
                var apiRequest = JsonConvert.DeserializeObject<ApiRequest>(item.Data, AuditLogWriteManager.SerializerSettings)!;
                if (apiRequest.IsCorrupted())
                {
                    saveCount++;
                    source.Remove(item);
                    continue;
                }
            }

            var headerId = await InserHeaderItemAsync(target, item);
            if (item.Files.Count > 0)
                await InsertFilesAsync(target, headerId, item.Files);

            if (typeof(TSourceType) == typeof(string))
                await insertItem(headerId, item.Data);
            else
                await insertItem(headerId, JsonConvert.DeserializeObject<TSourceType>(item.Data, AuditLogWriteManager.SerializerSettings)!);

            source.Remove(item);

            saveCount++;
            if (saveCount % 100 != 0)
                continue;
            await source.CommitAsync();
            saveCount = 0;
        }

        await source.CommitAsync();
    }

    private static async Task<Guid> InserHeaderItemAsync(NpgsqlConnection connection, AuditLogItem item)
    {
        var headerId = Guid.NewGuid();
        var createdAt = new DateTime(item.CreatedAt.Year, item.CreatedAt.Month, item.CreatedAt.Day, item.CreatedAt.Hour, item.CreatedAt.Minute, item.CreatedAt.Second, item.CreatedAt.Millisecond,
            DateTimeKind.Local).ToUniversalTime().ToString("o");
        var guildId = string.IsNullOrEmpty(item.GuildId) ? "NULL" : $"'{item.GuildId}'";
        var userId = string.IsNullOrEmpty(item.ProcessedUserId) ? "NULL" : $"'{item.ProcessedUserId}'";
        var channelId = string.IsNullOrEmpty(item.ChannelId) ? "NULL" : $"'{item.ChannelId}'";
        var discordId = string.IsNullOrEmpty(item.DiscordAuditLogItemId) ? "NULL" : $"'{item.DiscordAuditLogItemId}'";

        await using var command = connection.CreateCommand();
        command.CommandText = $"INSERT INTO \"LogItems\" (\"Id\", \"CreatedAt\", \"GuildId\", \"UserId\", \"ChannelId\", \"DiscordId\", \"Type\") VALUES " +
                              $"('{headerId}', '{createdAt}', {guildId}, {userId}, {channelId}, {discordId}, {(int)item.Type})";
        await command.ExecuteNonQueryAsync();

        return headerId;
    }

    private static async Task InsertFilesAsync(NpgsqlConnection connection, Guid headerId, IEnumerable<AuditLogFileMeta> files)
    {
        foreach (var file in files)
        {
            var fileId = Guid.NewGuid();

            await using var command = connection.CreateCommand();
            command.CommandText = $"INSERT INTO \"Files\" (\"Id\", \"LogItemId\", \"Filename\", \"Extension\", \"Size\")" +
                                  $" VALUES ('{fileId}', '{headerId}', '{file.Filename}', '{file.Extension}', {file.Size})";
            await command.ExecuteNonQueryAsync();
        }
    }
}
