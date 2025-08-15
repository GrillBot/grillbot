using System.IO.Compression;
using GrillBot.App.Helpers;
using GrillBot.App.Infrastructure.Jobs;
using GrillBot.Database.Entity;

namespace GrillBot.App.Jobs.Abstractions;

public abstract class ArchivationJobBase(IServiceProvider serviceProvider) : Job(serviceProvider)
{
    protected BlobManagerFactoryHelper BlobManagerFactoryHelper => ResolveService<BlobManagerFactoryHelper>();
    protected GrillBotDatabaseBuilder DatabaseBuilder => ResolveService<GrillBotDatabaseBuilder>();

    protected static JArray TransformGuilds(IEnumerable<Guild?> guilds)
    {
        var guildObjects = guilds
            .Where(o => o != null)
            .DistinctBy(o => o!.Id)
            .Select(o => new JObject
            {
                ["Id"] = o!.Id,
                ["Name"] = o.Name
            });

        return new JArray(guildObjects);
    }

    protected static JArray TransformUsers(IEnumerable<User?> users)
    {
        var userObjects = users
            .Where(o => o is not null)
            .DistinctBy(o => o!.Id)
            .Select(u => TransformUser(u!));

        return new JArray(userObjects);
    }

    protected static JArray TransformGuildUsers(IEnumerable<GuildUser> guildUsers)
    {
        var userObjects = guildUsers.DistinctBy(o => $"{o.UserId}/{o.GuildId}").Select(u =>
        {
            var user = TransformUser(u.User!);
            user["GuildId"] = u.GuildId;
            user["FullName"] = u.DisplayName ?? "";

            return user;
        });

        return new JArray(userObjects);
    }

    private static JObject TransformUser(User user)
    {
        var json = new JObject
        {
            ["Id"] = user.Id,
            ["FullName"] = user.Username
        };

        if (user.Flags > 0)
            json["Flags"] = user.Flags;

        return json;
    }

    protected static IEnumerable<JObject> TransformChannels(IEnumerable<GuildChannel?> channels)
    {
        return channels
            .Where(o => o is not null)
            .DistinctBy(o => $"{o!.ChannelId}/{o.GuildId}").Select(ch =>
            {
                var channel = new JObject
                {
                    ["Id"] = ch!.ChannelId,
                    ["Name"] = ch.Name,
                    ["Type"] = ch.ChannelType.ToString(),
                    ["GuildId"] = ch.GuildId
                };

                if (ch.UserPermissionsCount > 0)
                    channel["UserPermissionsCount"] = ch.UserPermissionsCount;
                if (ch.RolePermissionsCount > 0)
                    channel["RolePermissionsCount"] = ch.RolePermissionsCount;
                if (ch.Flags > 0)
                    channel["Flags"] = ch.Flags;
                if (!string.IsNullOrEmpty(ch.ParentChannelId))
                    channel["ParentChannelId"] = ch.ParentChannelId;

                return channel;
            });
    }

    protected static async Task AddJsonToZipAsync(ZipArchive archive, JObject json, string jsonName)
    {
        var entry = archive.CreateEntry(jsonName);
        entry.LastWriteTime = DateTimeOffset.Now;

        await using var entryStream = entry.Open();
        await using var streamWriter = new StreamWriter(entryStream);

        await streamWriter.WriteAsync(json.ToString(Formatting.None));
        await streamWriter.FlushAsync();
    }
}
