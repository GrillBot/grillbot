using System.Net.Http;
using GrillBot.Common.Services.KachnaOnline;
using GrillBot.Common.Services.KachnaOnline.Models;
using Moq;

namespace GrillBot.Tests.Infrastructure.Services;

public class KachnaOnlineClientBuilder : BuilderBase<IKachnaOnlineClient>
{
    public KachnaOnlineClientBuilder GetCurrentStateWithException()
    {
        Mock.Setup(o => o.GetCurrentStateAsync()).ThrowsAsync(new HttpRequestException());
        return this;
    }

    public KachnaOnlineClientBuilder GetCurrentStateWithoutException(DuckState state)
    {
        Mock.Setup(o => o.GetCurrentStateAsync()).ReturnsAsync(state);
        return this;
    }
}
