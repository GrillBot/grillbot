﻿using System.IO.Compression;
using GrillBot.App.Helpers;
using GrillBot.App.Infrastructure.Jobs;

namespace GrillBot.App.Jobs.Abstractions;

public abstract class ArchivationJobBase(IServiceProvider serviceProvider) : Job(serviceProvider)
{
    protected BlobManagerFactoryHelper BlobManagerFactoryHelper => ResolveService<BlobManagerFactoryHelper>();
    protected GrillBotDatabaseBuilder DatabaseBuilder => ResolveService<GrillBotDatabaseBuilder>();

    protected static JArray TransformGuilds(IEnumerable<Database.Entity.Guild?> guilds)
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

    protected static JArray TransformUsers(IEnumerable<Database.Entity.User?> users)
    {
        var userObjects = users
            .Where(o => o is not null)
            .DistinctBy(o => o!.Id)
            .Select(u => TransformUser(u!));

        return new JArray(userObjects);
    }

    protected static JArray TransformGuildUsers(IEnumerable<Database.Entity.GuildUser> guildUsers)
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

    private static JObject TransformUser(Database.Entity.User user)
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
