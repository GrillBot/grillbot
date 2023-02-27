using System.IO;
using System.Net;
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
public class DiscordExceptionHandlerTests : TestBase<DiscordExceptionHandler>
{
    private static IConfiguration Configuration => TestServices.Configuration.Value;

    private ITextChannel Channel { get; set; } = null!;
    private IUser User { get; set; } = null!;
    private TemporaryFile TemporaryFile { get; set; } = null!;

    protected override void PreInit()
    {
        Channel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).Build();
        User = new SelfUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

        TemporaryFile = new TemporaryFile("png");
        File.WriteAllBytes(TemporaryFile.Path, new byte[] { 1, 2, 3 });
    }

    protected override DiscordExceptionHandler CreateInstance()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName)
            .SetGetTextChannelsAction(new[] { Channel })
            .Build();

        var client = new ClientBuilder()
            .SetGetGuildsAction(new[] { guild })
            .SetSelfUser((ISelfUser)User)
            .Build();

        return new DiscordExceptionHandler(client, Configuration, TestServices.Provider.Value);
    }

    protected override void Cleanup()
    {
        TemporaryFile.Dispose();
    }

    [TestMethod]
    public async Task CanHandleAsync_NullException()
        => Assert.IsFalse(await Instance.CanHandleAsync(LogSeverity.Critical, ""));

    [TestMethod]
    public async Task CanHandleAsync_Disabled()
    {
        var oldValue = Configuration["Discord:Logging:Enabled"];
        Configuration["Discord:Logging:Enabled"] = "false";

        try
        {
            Assert.IsFalse(await Instance.CanHandleAsync(LogSeverity.Debug, "", new Exception()));
        }
        finally
        {
            Configuration["Discord:Logging:Enabled"] = oldValue;
        }
    }

    [TestMethod]
    public async Task CanHandleAsync_InvalidSeverity()
        => Assert.IsFalse(await Instance.CanHandleAsync(LogSeverity.Debug, "", new Exception()));

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
            new TaskCanceledException(),
            new HttpException(HttpStatusCode.ServiceUnavailable, null),
            new TimeoutException("Cannot defer an interaction")
        };

        foreach (var @case in cases)
            Assert.IsFalse(await Instance.CanHandleAsync(LogSeverity.Error, "Gateway", @case));
    }

    [TestMethod]
    public async Task CanHandleAsync_UnknownGuild()
    {
        var oldValue = Configuration["Discord:Logging:GuildId"];
        Configuration["Discord:Logging:GuildId"] = (Consts.GuildId + 1).ToString();

        try
        {
            ReflectionHelper.SetPrivateReadonlyPropertyValue(Instance, "LogChannel", null);
            Assert.IsFalse(await Instance.CanHandleAsync(LogSeverity.Critical, "", new Exception()));
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
            ReflectionHelper.SetPrivateReadonlyPropertyValue(Instance, "LogChannel", null);
            Assert.IsFalse(await Instance.CanHandleAsync(LogSeverity.Critical, "", new Exception()));
        }
        finally
        {
            Configuration["Discord:Logging:ChannelId"] = oldValue;
        }
    }

    [TestMethod]
    public async Task CanHandleAsync()
        => Assert.IsTrue(await Instance.CanHandleAsync(LogSeverity.Critical, "Test", new ArgumentException()));

    [TestMethod]
    public async Task InfoAsync()
        => await Instance.InfoAsync("Test", "Test");

    [TestMethod]
    public async Task WarningAsync()
    {
        const string source = "Test";
        const string message = "Test";
        var exception = new ArgumentException();

        await Instance.CanHandleAsync(LogSeverity.Critical, source, exception);
        await Instance.WarningAsync(source, message, exception);
    }

    [TestMethod]
    public async Task ErrorAsync_ApiException()
    {
        var innerException = new ArgumentException();
        var error = new ApiException("An error occured", innerException, User, "/api", "Test.Test");

        await Instance.CanHandleAsync(LogSeverity.Error, "API", error);
        await Instance.ErrorAsync("API", error.Message, error);
    }
}
