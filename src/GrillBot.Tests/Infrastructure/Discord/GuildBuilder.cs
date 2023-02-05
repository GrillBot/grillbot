using Discord;
using Moq;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace GrillBot.Tests.Infrastructure.Discord;

[ExcludeFromCodeCoverage]
public class GuildBuilder : BuilderBase<IGuild>
{
    private IRole EveryoneRole { get; }

    public GuildBuilder(ulong id, string name)
    {
        EveryoneRole = new RoleBuilder(id, "@everyone").Build();

        SetId(id);
        SetName(name);
        SetRoles(new[] { EveryoneRole });
        SetEveryoneRole(EveryoneRole);
        SetFeatures(GuildFeature.None);
        SetGetAuditLogsAction(new List<IAuditLogEntry>());
    }

    public GuildBuilder(IGuild guild) : this(guild.Id, guild.Name)
    {
        SetRoles(guild.Roles);
        SetEveryoneRole(guild.EveryoneRole);
    }

    public GuildBuilder SetId(ulong id)
    {
        Mock.Setup(o => o.Id).Returns(id);
        Mock.Setup(o => o.CreatedAt).Returns(SnowflakeUtils.FromSnowflake(id));
        return this;
    }

    public GuildBuilder SetName(string name)
    {
        Mock.Setup(o => o.Name).Returns(name);
        return this;
    }

    public GuildBuilder SetRoles(IEnumerable<IRole> roles)
    {
        var rolesData = roles.ToList();
        if (!rolesData.Exists(o => o.Id == EveryoneRole.Id))
            rolesData.Add(EveryoneRole);

        Mock.Setup(o => o.Roles).Returns(rolesData.AsReadOnly());
        foreach (var role in rolesData.Where(o => o.Id != EveryoneRole.Id))
            SetGetRoleAction(role);
        return this;
    }

    public GuildBuilder SetEveryoneRole(IRole role)
    {
        Mock.Setup(o => o.EveryoneRole).Returns(role);
        SetGetRoleAction(role);
        return this;
    }

    public GuildBuilder SetGetRoleAction(IRole role)
    {
        Mock.Setup(o => o.GetRole(It.Is<ulong>(id => id == role.Id))).Returns(role);
        return this;
    }

    public GuildBuilder SetGetTextChannelsAction(IEnumerable<ITextChannel> channels)
    {
        var channelsData = channels.ToList().AsReadOnly();

        Mock.Setup(o => o.GetTextChannelsAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync(channelsData);
        Mock.Setup(o => o.GetChannelsAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync(channelsData);
        foreach (var channel in channelsData)
        {
            Mock.Setup(o => o.GetTextChannelAsync(It.Is<ulong>(x => x == channel.Id), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync(channel);
            Mock.Setup(o => o.GetChannelAsync(It.Is<ulong>(x => x == channel.Id), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync(channel);
        }

        return this;
    }

    public GuildBuilder SetGetCategoriesAction(IEnumerable<ICategoryChannel> categories)
    {
        var categoryData = categories.ToList().AsReadOnly();

        Mock.Setup(o => o.GetCategoriesAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync(categoryData);
        foreach (var category in categoryData)
            Mock.Setup(o => o.GetChannelAsync(It.Is<ulong>(id => id == category.Id), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync(category);
        return this;
    }

    public GuildBuilder SetGetUsersAction(IEnumerable<IGuildUser> users)
    {
        var usersData = users.ToList().AsReadOnly();

        Mock.Setup(o => o.GetUsersAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync(usersData);
        foreach (var user in usersData)
            Mock.Setup(o => o.GetUserAsync(It.Is<ulong>(id => id == user.Id), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync(user);

        return this;
    }

    public GuildBuilder SetGetCurrentUserAction(IGuildUser user)
    {
        Mock.Setup(o => o.GetCurrentUserAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync(user);
        Mock.Setup(o => o.GetUserAsync(It.Is<ulong>(x => x == user.Id), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync(user);
        return this;
    }

    public GuildBuilder SetGetInvitesAction(IEnumerable<IInviteMetadata> invites)
    {
        var invitesData = invites.ToList();
        var vanityInvite = invitesData.Find(o => o.Code == Consts.VanityInviteCode);
        invitesData.RemoveAll(o => o.Code == Consts.VanityInviteCode);

        Mock.Setup(o => o.GetInvitesAsync(It.IsAny<RequestOptions>())).ReturnsAsync(invitesData.AsReadOnly());
        if (vanityInvite != null)
            SetGetVanityInviteAsync(vanityInvite);
        return this;
    }

    public GuildBuilder SetGetVanityInviteAsync(IInviteMetadata invite)
    {
        Mock.Setup(o => o.VanityURLCode).Returns(invite.Code);
        Mock.Setup(o => o.GetVanityInviteAsync(It.IsAny<RequestOptions>())).ReturnsAsync(invite);
        return this;
    }

    public GuildBuilder SetEmotes(IEnumerable<GuildEmote> emotes)
    {
        var data = emotes.ToList().AsReadOnly();
        Mock.Setup(o => o.Emotes).Returns(data);
        Mock.Setup(o => o.GetEmotesAsync(It.IsAny<RequestOptions>())).ReturnsAsync(data);
        return this;
    }

    public GuildBuilder SetCreateEventAction(IGuildScheduledEvent @event)
    {
        Mock.Setup(o => o.CreateEventAsync(It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<GuildScheduledEventType>(), It.IsAny<GuildScheduledEventPrivacyLevel>(), It.IsAny<string>(),
            It.IsAny<DateTimeOffset?>(), It.IsAny<ulong?>(), It.IsAny<string>(), It.IsAny<Image?>(), It.IsAny<RequestOptions>())).ReturnsAsync(@event);
        return this;
    }

    public GuildBuilder SetGetEventsAction(IEnumerable<IGuildScheduledEvent> events)
    {
        var eventsData = events.ToList().AsReadOnly();
        Mock.Setup(o => o.GetEventsAsync(It.IsAny<RequestOptions>())).ReturnsAsync(eventsData);

        foreach (var @event in eventsData)
            Mock.Setup(o => o.GetEventAsync(It.Is<ulong>(x => x == @event.Id), It.IsAny<RequestOptions>())).ReturnsAsync(@event);
        return this;
    }

    public GuildBuilder SetGetAuditLogsAction(IReadOnlyCollection<IAuditLogEntry> entries)
    {
        Mock.Setup(o => o.GetAuditLogsAsync(It.IsAny<int>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>(), It.IsAny<ulong?>(), It.IsAny<ulong?>(), It.IsAny<ActionType?>())).ReturnsAsync(entries);
        return this;
    }

    public GuildBuilder SetFeatures(GuildFeature value)
    {
        var guildFeatures = ReflectionHelper.CreateWithInternalConstructor<GuildFeatures>(value, Array.Empty<string>());

        Mock.Setup(o => o.Features).Returns(guildFeatures);
        return this;
    }

    public GuildBuilder SetGetBansAction(IReadOnlyCollection<IBan> bans)
    {
        var banList = new List<IReadOnlyCollection<IBan>> { bans }.ToAsyncEnumerable();
        Mock.Setup(o => o.GetBansAsync(It.IsAny<int>(), It.IsAny<RequestOptions>())).Returns(banList);
        foreach (var ban in bans)
        {
            Mock.Setup(o => o.GetBanAsync(It.Is<IUser>(b => b.Id == ban.User.Id), It.IsAny<RequestOptions>())).ReturnsAsync(ban);
            Mock.Setup(o => o.GetBanAsync(It.Is<ulong>(id => id == ban.User.Id), It.IsAny<RequestOptions>())).ReturnsAsync(ban);
        }

        return this;
    }

    public GuildBuilder SetGetVoiceChannelsAction(IEnumerable<IVoiceChannel> channels)
    {
        var channelData = channels.ToList().AsReadOnly();
        Mock.Setup(o => o.GetVoiceChannelsAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync(channelData);
        foreach (var channel in channelData)
        {
            Mock.Setup(o => o.GetChannelAsync(It.Is<ulong>(id => id == channel.Id), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync(channel);
            Mock.Setup(o => o.GetVoiceChannelAsync(It.Is<ulong>(id => id == channel.Id), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync(channel);
        }

        return this;
    }

    public GuildBuilder SetOwner(IGuildUser user)
    {
        Mock.Setup(o => o.OwnerId).Returns(user.Id);
        Mock.Setup(o => o.GetOwnerAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync(user);
        return this;
    }

    public GuildBuilder SetDescription(string description)
    {
        Mock.Setup(o => o.Description).Returns(description);
        return this;
    }

    public GuildBuilder SetBanner(string bannerId, string bannerUrl)
    {
        Mock.Setup(o => o.BannerId).Returns(bannerId);
        Mock.Setup(o => o.BannerUrl).Returns(bannerUrl);
        return this;
    }
}
