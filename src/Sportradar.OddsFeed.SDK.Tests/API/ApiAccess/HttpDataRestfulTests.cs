using System;
using Sportradar.OddsFeed.SDK.Api.Internal.Replay;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Api.ApiAccess;

public class HttpDataRestfulTests
{
    private readonly Uri _badUri = new("http://www.unexisting-url.com");
    private readonly Uri _putUri = new("http://httpbin.org/put");
    private readonly Uri _deleteUri = new("http://httpbin.org/delete");

    private readonly HttpDataRestful _httpDataRestful;

    public HttpDataRestfulTests()
    {
        var config = TestConfiguration.GetConfig();

        _httpDataRestful = new HttpDataRestful(new TestHttpClient(), new Deserializer<response>());
    }

    //TODO requires network
    //[Fact]
    public void PutDataAsyncTest()
    {
        // in logRest file there should be result for this call
        var result = _httpDataRestful.PutDataAsync(_putUri).GetAwaiter().GetResult();
        Assert.NotNull(result);
        Assert.True(result.IsSuccessStatusCode);
    }

    //[Fact]
    public void PutDataAsyncTestWithWrongUrl()
    {
        // in logRest file there should be result for this call
        var ex = Assert.Throws<AggregateException>(() => _httpDataRestful.PutDataAsync(_badUri).GetAwaiter().GetResult());
        Assert.IsType<AggregateException>(ex);
        if (ex.InnerException != null)
        {
            Assert.IsType<CommunicationException>(ex.InnerException);
        }
    }

    //TODO requires network
    //[Fact]
    public void DeleteDataAsyncTest()
    {
        var result = _httpDataRestful.DeleteDataAsync(_deleteUri).GetAwaiter().GetResult();
        Assert.NotNull(result);
        Assert.True(result.IsSuccessStatusCode);
    }

    //[Fact]
    public void DeleteDataAsyncTestWithWrongUrl()
    {
        var ex = Assert.Throws<AggregateException>(() => _httpDataRestful.DeleteDataAsync(_badUri).GetAwaiter().GetResult());
        Assert.IsType<AggregateException>(ex);
        if (ex.InnerException != null)
        {
            Assert.IsType<CommunicationException>(ex.InnerException);
        }
    }
}
