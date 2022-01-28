using Discord.WebSocket;
using GrillBot.App.Controllers;
using GrillBot.App.Services.Discord;
using GrillBot.App.Services.MessageCache;
using GrillBot.Database.Services;
using GrillBot.Tests.TestHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace GrillBot.Tests.App.Controllers
{
    [TestClass]
    public class ChannelControllerTests
    {
        private static ServiceProvider CreateController(out ChannelController controller)
        {
            var client = new DiscordSocketClient();
            var container = DIHelpers.CreateContainer(services =>
            {
                services.AddSingleton(new DiscordInitializationService(NullLogger<DiscordInitializationService>.Instance));
                services.AddSingleton<MessageCache>();
                services.AddSingleton(client);
            });
            var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));
            var messageCache = container.GetService<MessageCache>();

            controller = new ChannelController(client, dbContext, messageCache);
            return container;
        }

        [TestMethod]
        public void GetChannelBoard_NoMutualGuilds()
        {
            using var _ = CreateController(out var controller);

            var result = controller.GetChannelboardAsync().Result;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public void GetChannelDetail_NotFound()
        {
            using var container = CreateController(out var controller);
            var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));
            dbContext.Channels.RemoveRange(dbContext.Channels.AsEnumerable());
            dbContext.SaveChanges();

            var result = controller.GetChannelDetailAsync(1).Result;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public void GetChannelDetail_Found_WithoutActivity()
        {
            using var container = CreateController(out var controller);
            var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));
            dbContext.Channels.RemoveRange(dbContext.Channels.AsEnumerable());
            dbContext.Guilds.RemoveRange(dbContext.Guilds.AsEnumerable());
            dbContext.UserChannels.RemoveRange(dbContext.UserChannels.AsEnumerable());
            dbContext.Users.RemoveRange(dbContext.Users.AsEnumerable());
            dbContext.GuildUsers.RemoveRange(dbContext.GuildUsers.AsEnumerable());
            dbContext.Guilds.Add(new GrillBot.Database.Entity.Guild() { Id = "2", Name = "Name" });
            dbContext.Channels.Add(new GrillBot.Database.Entity.GuildChannel()
            {
                ChannelId = "1",
                ChannelType = Discord.ChannelType.Text,
                Name = "Channel",
                GuildId = "2"
            });
            dbContext.SaveChanges();

            var result = controller.GetChannelDetailAsync(1).Result;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public void GetChannelDetail_Found_WithActivity()
        {
            using var container = CreateController(out var controller);
            var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));
            dbContext.Channels.RemoveRange(dbContext.Channels.AsEnumerable());
            dbContext.Guilds.RemoveRange(dbContext.Guilds.AsEnumerable());
            dbContext.UserChannels.RemoveRange(dbContext.UserChannels.AsEnumerable());
            dbContext.Users.RemoveRange(dbContext.Users.AsEnumerable());
            dbContext.GuildUsers.RemoveRange(dbContext.GuildUsers.AsEnumerable());
            dbContext.Users.Add(new GrillBot.Database.Entity.User() { Id = "3", Username = "Username", Discriminator = "9999" });
            dbContext.Guilds.Add(new GrillBot.Database.Entity.Guild() { Id = "2", Name = "Name" });
            dbContext.GuildUsers.Add(new GrillBot.Database.Entity.GuildUser() { GuildId = "2", UserId = "3" });
            var channel = new GrillBot.Database.Entity.GuildChannel()
            {
                ChannelId = "1",
                ChannelType = Discord.ChannelType.Text,
                Name = "Channel",
                GuildId = "2",
            };
            channel.Users.Add(new GrillBot.Database.Entity.GuildUserChannel()
            {
                Count = 50,
                FirstMessageAt = DateTime.MaxValue,
                GuildId = "2",
                Id = "1",
                LastMessageAt = DateTime.MaxValue,
                UserId = "3"
            });
            dbContext.Channels.Add(channel);
            dbContext.SaveChanges();

            var result = controller.GetChannelDetailAsync(1).Result;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }
    }
}
