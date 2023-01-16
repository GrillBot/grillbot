﻿using Discord;
using Moq;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace GrillBot.Tests.Infrastructure.Discord;

[ExcludeFromCodeCoverage]
public class ClientBuilder : BuilderBase<IDiscordClient>
{
    public ClientBuilder SetSelfUser(ISelfUser user)
    {
        Mock.Setup(o => o.CurrentUser).Returns(user);
        return this;
    }

    public ClientBuilder SetGetGuildsAction(IEnumerable<IGuild> guilds)
    {
        var guildList = guilds.ToList().AsReadOnly();

        Mock.Setup(o => o.GetGuildsAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult((IReadOnlyCollection<IGuild>)guildList));
        foreach (var guild in guildList)
            Mock.Setup(o => o.GetGuildAsync(It.Is<ulong>(x => x == guild.Id), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(guild));

        return this;
    }

    public ClientBuilder SetGetUserAction(IUser user)
    {
        Mock.Setup(o => o.GetUserAsync(It.Is<ulong>(x => x == user.Id), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(user));
        return this;
    }

    public ClientBuilder SetGetUserAction(IEnumerable<IUser> users)
    {
        foreach (var user in users)
            SetGetUserAction(user);
        return this;
    }
}
