using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using GrillBot.App.Handlers.Logging;
using GrillBot.App.Infrastructure.IO;
using GrillBot.Common.Exceptions;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Handlers.Logging;

[TestClass]
public class DiscordExceptionHandlerTests : ServiceTest<DiscordExceptionHandler>
{
    private static IConfiguration Configuration => TestServices.Configuration.Value;

    private ITextChannel Channel { get; set; }
    private IUser User { get; set; }
    private TemporaryFile TemporaryFile { get; set; }

    protected override DiscordExceptionHandler CreateService()
    {
        Channel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).Build();

        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName)
            .SetGetTextChannelAction(Channel)
            .Build();

        User = new SelfUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

        var client = new ClientBuilder()
            .SetGetGuildAction(guild)
            .SetSelfUser((ISelfUser)User)
            .Build();

        TemporaryFile = new TemporaryFile("png");
        File.WriteAllBytes(TemporaryFile.Path, new byte[] { 1, 2, 3 });

        return new DiscordExceptionHandler(client, Configuration, TestServices.InitializedProvider.Value);
    }

    public override void Cleanup()
    {
        if (File.Exists("LastErrorDateTest.txt"))
            File.Delete("LastErrorDateTest.txt");

        TemporaryFile.Dispose();
    }

    [TestMethod]
    public async Task CanHandleAsync_NullException()
    {
        var result = await Service.CanHandleAsync(LogSeverity.Critical, "");
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task CanHandleAsync_Disabled()
    {
        var oldValue = Configuration["Discord:Logging:Enabled"];
        Configuration["Discord:Logging:Enabled"] = "false";

        try
        {
            var result = await Service.CanHandleAsync(LogSeverity.Debug, "", new Exception());
            Assert.IsFalse(result);
        }
        finally
        {
            Configuration["Discord:Logging:Enabled"] = oldValue;
        }
    }

    [TestMethod]
    public async Task CanHandleAsync_InvalidSeverity()
    {
        var result = await Service.CanHandleAsync(LogSeverity.Debug, "", new Exception());
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task CanHandleAsync_IgnoredExceptions()
    {
        var resourceUnavailable = new SocketException();
        ReflectionHelper.SetPrivateReadonlyPropertyValue(resourceUnavailable, "NativeErrorCode", 11);

        var cases = new Exception[]
        {
            new GatewayReconnectException(""),
            new("", new GatewayReconnectException("")),
            new("Server missed last heartbeat"),
            new TaskCanceledException("", new IOException("", new SocketException((int)SocketError.ConnectionAborted))),
            new HttpRequestException("", resourceUnavailable),
            new("", new WebSocketException()),
            new("", new WebSocketClosedException(0)),
            new TaskCanceledException()
        };

        foreach (var @case in cases)
        {
            var result = await Service.CanHandleAsync(LogSeverity.Error, "Gateway", @case);
            Assert.IsFalse(result);
        }
    }

    [TestMethod]
    public async Task CanHandleAsync_UnknownGuild()
    {
        var oldValue = Configuration["Discord:Logging:GuildId"];
        Configuration["Discord:Logging:GuildId"] = (Consts.GuildId + 1).ToString();

        try
        {
            ReflectionHelper.SetPrivateReadonlyPropertyValue(Service, "LogChannel", null);
            var result = await Service.CanHandleAsync(LogSeverity.Critical, "", new Exception());
            Assert.IsFalse(result);
        }
        finally
        {
            Configuration["Discord:Logging:GuildId"] = oldValue;
        }
    }

    [TestMethod]
    public async Task CanHandleAsync_UnknownChannel()
    {
        var oldValue = Configuration["Discord:Logging:ChannelId"];
        Configuration["Discord:Logging:ChannelId"] = (Consts.ChannelId + 1).ToString();

        try
        {
            ReflectionHelper.SetPrivateReadonlyPropertyValue(Service, "LogChannel", null);
            var result = await Service.CanHandleAsync(LogSeverity.Critical, "", new Exception());
            Assert.IsFalse(result);
        }
        finally
        {
            Configuration["Discord:Logging:ChannelId"] = oldValue;
        }
    }

    [TestMethod]
    public async Task CanHandleAsync()
    {
        var result = await Service.CanHandleAsync(LogSeverity.Critical, "Test", new ArgumentException());
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task InfoAsync()
    {
        await Service.InfoAsync("Test", "Test");
    }

    [TestMethod]
    public async Task WarningAsync()
    {
        const string source = "Test";
        const string message = "Test";
        var exception = new ArgumentException();

        await Service.CanHandleAsync(LogSeverity.Critical, source, exception);
        await Service.WarningAsync(source, message, exception);
    }

    [TestMethod]
    public async Task ErrorAsync_ApiException()
    {
        var innerException = new ArgumentException();
        var error = new ApiException("An error occured", innerException, User, "/api", "Test.Test");

        await Service.CanHandleAsync(LogSeverity.Error, "API", error);
        await Service.ErrorAsync("API", error.Message, error);
    }
}
