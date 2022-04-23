using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using GrillBot.App;
using GrillBot.App.Infrastructure.TypeReaders;
using Moq;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.TestHelpers;

[ExcludeFromCodeCoverage]
public static class DiscordHelper
{
    public static DiscordSocketClient CreateClient()
    {
        return new DiscordSocketClient();
    }

    public static IDiscordClient CreateDiscordClient()
    {
        var mock = new Mock<IDiscordClient>();

        var guildUser = DataHelper.CreateGuildUser();
        var self = DataHelper.CreateSelfUser();

        var guild = DataHelper.CreateGuild(mock =>
        {
            mock.Setup(o => o.GetUserAsync(It.Is<ulong>(x => x == guildUser.Id), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(guildUser));
            mock.Setup(o => o.GetUserAsync(It.Is<ulong>(x => x == self.Id), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(DataHelper.CreateGuildUser(id: self.Id)));
        });

        mock.Setup(o => o.CurrentUser).Returns(self);
        mock.Setup(o => o.GetGuildsAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .Returns(Task.FromResult(new List<IGuild>() { guild }.AsReadOnly() as IReadOnlyCollection<IGuild>));
        mock.Setup(o => o.GetGuildAsync(It.Is<ulong>(o => o == guild.Id), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .Returns(Task.FromResult(guild));

        return mock.Object;
    }

    public static CommandService CreateCommandsService(bool init = false)
    {
        var service = new CommandService();

        if (init)
        {
            var provider = DIHelper.CreateInitializedProvider();

            service.RegisterTypeReaders();
            service.AddModulesAsync(typeof(Startup).Assembly, provider).Wait();
        }

        return service;
    }

    public static InteractionService CreateInteractionService(DiscordSocketClient discordClient, bool init = false)
    {
        var service = new InteractionService(discordClient);

        if (init)
        {
            var provider = DIHelper.CreateInitializedProvider();

            service.RegisterTypeConverters();
            service.AddModulesAsync(typeof(Startup).Assembly, provider).Wait();
        }

        return service;
    }
}
