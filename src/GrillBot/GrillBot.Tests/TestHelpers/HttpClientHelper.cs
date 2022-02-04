using Moq;
using Moq.Protected;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GrillBot.Tests.TestHelpers;

[ExcludeFromCodeCoverage]
public static class HttpClientHelper
{
    public static IHttpClientFactory CreateFactory(HttpResponseMessage response)
    {
        var httpHandlerMock = new Mock<HttpMessageHandler>();
        httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            ).ReturnsAsync(response)
            .Verifiable();

        var mock = new Mock<IHttpClientFactory>();
        mock.Setup(o => o.CreateClient(It.IsAny<string>())).Returns(new HttpClient(httpHandlerMock.Object));
        return mock.Object;
    }
}
