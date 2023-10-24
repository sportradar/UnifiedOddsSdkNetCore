/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest;

public class HttpDataFetcherTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly SdkHttpClient _sdkHttpClient;
    private readonly HttpDataFetcher _httpDataFetcher;
    private readonly Uri _badUri = new("http://www.unexisting-url.com");
    private readonly Uri _getUri = new("http://test.domain.com/get");
    private readonly Uri _postUri = new("http://test.domain.com/post");
    private readonly StubMessageHandler _stubMessageHandler;

    public HttpDataFetcherTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _stubMessageHandler = new StubMessageHandler(_outputHelper, 100, 50);
        var httpClient = new HttpClient(_stubMessageHandler);
        httpClient.DefaultRequestHeaders.Add(UofSdkBootstrap.HttpClientDefaultRequestHeaderForAccessToken, "aaa");
        httpClient.DefaultRequestHeaders.Add(UofSdkBootstrap.HttpClientDefaultRequestHeaderForUserAgent, "UofSdk-Net");
        _sdkHttpClient = new SdkHttpClient(httpClient);

        _httpDataFetcher = new HttpDataFetcher(_sdkHttpClient, new Deserializer<response>());
    }

    [Fact]
    public void Constructor_WithoutSdkHttpClient_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new HttpDataFetcher(null, new Deserializer<response>()));
    }

    [Fact]
    public void Constructor_WithoutDeserializer_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new HttpDataFetcher(_sdkHttpClient, null));
    }

    [Fact]
    public void Constructor_InvalidConnectionFailureLimit_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new HttpDataFetcher(_sdkHttpClient, new Deserializer<response>(), 0));
    }

    [Fact]
    public void Constructor_InvalidConnectionFailureTimeout_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new HttpDataFetcher(_sdkHttpClient, new Deserializer<response>(), 15, 0));
    }

    [Fact]
    public async Task GetDataAsync_NormalUri()
    {
        var result = await _httpDataFetcher.GetDataAsync(_getUri);

        Assert.NotNull(result);
        Assert.True(result.CanRead);

        var s = await new StreamReader(result).ReadToEndAsync();
        Assert.True(!string.IsNullOrEmpty(s));
    }

    [Fact]
    public void ValidateConnection_ConnectionFailureTime()
    {
        _stubMessageHandler.SetWantedResponse(new HttpResponseMessage(HttpStatusCode.BadGateway));
        var httpDataFetcher = new HttpDataFetcher(_sdkHttpClient, new Deserializer<response>(), 1, 5);
        Stream result = null;
        Assert.Throws<CommunicationException>(() => result = httpDataFetcher.GetDataAsync(_getUri).GetAwaiter().GetResult());
        Assert.Null(result);

        var e = Assert.Throws<CommunicationException>(() => result = httpDataFetcher.GetDataAsync(_getUri).GetAwaiter().GetResult());

        Assert.Null(result);
        Assert.IsType<CommunicationException>(e);
        Assert.Null(e.InnerException);
        Assert.Equal("Failed to execute request due to previous failures.", e.Message);
    }

    [Fact]
    public async Task ValidateConnection_ConnectionFailureTime_SuccessfulResetAfterTimeout()
    {
        _stubMessageHandler.SetWantedResponse(new HttpResponseMessage(HttpStatusCode.BadGateway));
        var httpDataFetcher = new HttpDataFetcher(_sdkHttpClient, new Deserializer<response>(), 1, 1);
        Stream result = null;

        Assert.Throws<CommunicationException>(() => result = httpDataFetcher.GetDataAsync(_getUri).GetAwaiter().GetResult());
        Assert.Null(result);

        _stubMessageHandler.SetWantedResponse(GetSuccessResponseMessage());
        await Task.Delay(TimeSpan.FromSeconds(1));

        result = httpDataFetcher.GetDataAsync(_getUri).GetAwaiter().GetResult();
        Assert.NotNull(result);
    }

    [Fact]
    public void GetDataAsync_WrongUrl_ReturnsBadGatewayForWrongMessage()
    {
        _stubMessageHandler.SetWantedResponse(new HttpResponseMessage(HttpStatusCode.BadGateway));
        Stream result = null;

        var e = Assert.Throws<CommunicationException>(() => result = _httpDataFetcher.GetDataAsync(_badUri).GetAwaiter().GetResult());

        Assert.Null(result);
        Assert.IsType<CommunicationException>(e);
        if (e.InnerException != null)
        {
            Assert.IsType<DeserializationException>(e.InnerException);
        }
    }

    [Fact]
    public void GetDataAsync_WrongUrl_ThrowsHttpRequestException()
    {
        _stubMessageHandler.SetWantedResponse(new HttpRequestException("any-message", new SocketException(), HttpStatusCode.BadGateway));
        Stream result = null;

        var e = Assert.Throws<CommunicationException>(() => result = _httpDataFetcher.GetDataAsync(_badUri).GetAwaiter().GetResult());

        Assert.Null(result);
        Assert.IsType<CommunicationException>(e);
        if (e.InnerException != null)
        {
            Assert.IsType<HttpRequestException>(e.InnerException);
        }
    }

    [Fact]
    public void GetData_BadGatewayWithValidContent()
    {
        _stubMessageHandler.SetWantedResponse(GetSuccessResponseMessage(HttpStatusCode.GatewayTimeout));
        Stream result = null;

        var e = Assert.Throws<CommunicationException>(() => result = _httpDataFetcher.GetData(_getUri));

        Assert.Null(result);
        Assert.IsType<CommunicationException>(e);
        Assert.Null(e.InnerException);
    }

    [Fact]
    public void GetData_WrongUrl_ThrowsSocketException()
    {
        _stubMessageHandler.SetWantedResponse(new SocketException());
        Stream result = null;

        var e = Assert.Throws<CommunicationException>(() => result = _httpDataFetcher.GetData(_getUri));

        Assert.Null(result);
        Assert.IsType<CommunicationException>(e);
        if (e.InnerException != null)
        {
            Assert.IsType<SocketException>(e.InnerException);
        }
    }

    [Fact]
    public void GetData_ValidContentWithResponseHeaders()
    {
        var httpResponseMessage = GetSuccessResponseMessage();
        httpResponseMessage.Headers.Add("any-key", "some-value");
        _stubMessageHandler.SetWantedResponse(httpResponseMessage);

        var result = _httpDataFetcher.GetData(_getUri);

        Assert.NotNull(result);
        Assert.NotNull(_httpDataFetcher.ResponseHeaders);
        Assert.NotEmpty(_httpDataFetcher.ResponseHeaders);
        Assert.Equal("any-key", _httpDataFetcher.ResponseHeaders.First().Key);
    }

    [Fact]
    public void GetData_ValidContentWithMultipleSameResponseHeaders()
    {
        var httpResponseMessage = GetSuccessResponseMessage();
        httpResponseMessage.Headers.Add("any-key", "some-value");
        httpResponseMessage.Headers.Add("any-key", "second-value");
        _stubMessageHandler.SetWantedResponse(httpResponseMessage);

        var result = _httpDataFetcher.GetData(_getUri);

        Assert.NotNull(result);
        Assert.NotNull(_httpDataFetcher.ResponseHeaders);
        Assert.NotEmpty(_httpDataFetcher.ResponseHeaders);
        Assert.Single(_httpDataFetcher.ResponseHeaders);
        Assert.Equal("any-key", _httpDataFetcher.ResponseHeaders.First().Key);
    }

    [Fact]
    public void GetData_NullContent_Throws()
    {
        var httpResponseMessage = GetSuccessResponseMessage();
        httpResponseMessage.Content = null;
        _stubMessageHandler.SetWantedResponse(httpResponseMessage);
        Stream result = null;

        var e = Assert.Throws<CommunicationException>(() => result = _httpDataFetcher.GetData(_getUri));

        Assert.Null(result);
        Assert.IsType<CommunicationException>(e);
        Assert.Null(e.InnerException);
        Assert.Equal("Missing content in the response", e.Message);
    }

    [Fact]
    public async Task PostDataAsync_NormalUri()
    {
        var result = await _httpDataFetcher.PostDataAsync(_postUri);

        Assert.NotNull(result);
        Assert.True(result.IsSuccessStatusCode);
    }

    [Fact]
    public async Task PostDataAsync_WithContent()
    {
        var result = await _httpDataFetcher.PostDataAsync(_getUri, new StringContent("test string"));

        Assert.NotNull(result);
        Assert.True(result.IsSuccessStatusCode);
    }

    [Fact]
    public async Task PostDataAsync_EmptyResponseContent()
    {
        _stubMessageHandler.SetWantedResponse(new HttpResponseMessage(HttpStatusCode.Accepted)
        {
            Content = new ByteArrayContent(Array.Empty<byte>())
        });
        var result = await _httpDataFetcher.PostDataAsync(_postUri);

        Assert.NotNull(result);
        Assert.True(result.IsSuccessStatusCode);
    }

    [Fact]
    public void PostDataAsync_TestWithWrongUrl()
    {
        _stubMessageHandler.SetWantedResponse(new HttpRequestException("any-message", new SocketException(), HttpStatusCode.BadGateway));
        HttpResponseMessage result = null;

        var ex = Assert.Throws<CommunicationException>(() => result = _httpDataFetcher.PostDataAsync(_badUri).GetAwaiter().GetResult());

        Assert.Null(result);
        Assert.IsType<CommunicationException>(ex);
        if (ex.InnerException != null)
        {
            Assert.IsType<HttpRequestException>(ex.InnerException);
        }
    }

    [Fact]
    public void PostDataAsync_WrongUrl_ThrowCommunicationException()
    {
        _stubMessageHandler.SetWantedResponse(new CommunicationException("any-message", _badUri.PathAndQuery, HttpStatusCode.BadGateway, null));
        HttpResponseMessage result = null;

        var e = Assert.Throws<CommunicationException>(() => result = _httpDataFetcher.PostDataAsync(_badUri).GetAwaiter().GetResult());

        Assert.Null(result);
        Assert.IsType<CommunicationException>(e);
        Assert.Null(e.InnerException);
    }

    [Fact]
    public async Task PostDataAsync_ValidContentWithResponseHeaders()
    {
        var httpResponseMessage = GetSuccessResponseMessage();
        httpResponseMessage.Headers.Add("any-key", "some-value");
        _stubMessageHandler.SetWantedResponse(httpResponseMessage);

        var result = await _httpDataFetcher.PostDataAsync(_getUri);

        Assert.NotNull(result);
        Assert.NotNull(_httpDataFetcher.ResponseHeaders);
        Assert.NotEmpty(_httpDataFetcher.ResponseHeaders);
        Assert.Equal("any-key", _httpDataFetcher.ResponseHeaders.First().Key);
    }

    [Fact]
    public async Task PostDataAsync_ValidContentWithMultipleSameResponseHeaders()
    {
        var httpResponseMessage = GetSuccessResponseMessage();
        httpResponseMessage.Headers.Add("any-key", "some-value");
        httpResponseMessage.Headers.Add("any-key", "second-value");
        _stubMessageHandler.SetWantedResponse(httpResponseMessage);

        var result = await _httpDataFetcher.PostDataAsync(_getUri);

        Assert.NotNull(result);
        Assert.NotNull(_httpDataFetcher.ResponseHeaders);
        Assert.NotEmpty(_httpDataFetcher.ResponseHeaders);
        Assert.Single(_httpDataFetcher.ResponseHeaders);
        Assert.Equal("any-key", _httpDataFetcher.ResponseHeaders.First().Key);
    }

    private HttpResponseMessage GetSuccessResponseMessage(HttpStatusCode httpStatusCode = HttpStatusCode.Accepted)
    {
        var response = GetResponse(httpStatusCode);
        var responseContent = ObjectToXmlString(response);

        return new HttpResponseMessage(httpStatusCode) { Content = new StringContent(responseContent) };
    }

    private response GetResponse(HttpStatusCode httpStatusCode = HttpStatusCode.Accepted)
    {
        return new response { action = "some-response-action", message = $"some-response-message {httpStatusCode}", response_code = response_code.ACCEPTED, response_codeSpecified = true };
    }

    public static string ObjectToXmlString<T>(T objectToSerialize)
    {
        var xmlSerializer = new XmlSerializer(typeof(T));

        using var memoryStream = new MemoryStream();
        // Use UTF8 Encoding but omit the Byte Order Mark (BOM)
        using var writer = new StreamWriter(memoryStream, new UTF8Encoding(false));
        xmlSerializer.Serialize(writer, objectToSerialize);

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }
}
