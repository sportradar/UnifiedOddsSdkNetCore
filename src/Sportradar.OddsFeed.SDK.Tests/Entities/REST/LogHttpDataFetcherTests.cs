// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest;

[Collection(NonParallelCollectionFixture.NonParallelTestCollection)]
public class LogHttpDataFetcherTests
{
    private const string HttpClientDefaultName = "test";
    private readonly SdkHttpClient _sdkHttpClient;
    private readonly LogHttpDataFetcher _logHttpDataFetcher;
    private readonly Uri _badUri = new Uri("http://www.unexisting-url.com");
    private readonly Uri _getUri = new Uri("http://test.domain.com/get");
    private readonly Uri _postUri = new Uri("http://test.domain.com/post");
    private readonly StubMessageHandler _stubMessageHandler;

    public LogHttpDataFetcherTests(ITestOutputHelper outputHelper)
    {
        _stubMessageHandler = new StubMessageHandler(outputHelper, 100, 50);
        var services = new ServiceCollection();
        services.AddHttpClient(HttpClientDefaultName)
                .ConfigureHttpClient(configureClient =>
                                     {
                                         configureClient.Timeout = TimeSpan.FromSeconds(5);
                                         configureClient.DefaultRequestHeaders.Add(UofSdkBootstrap.HttpClientDefaultRequestHeaderForAccessToken, "aaa");
                                         configureClient.DefaultRequestHeaders.Add(UofSdkBootstrap.HttpClientDefaultRequestHeaderForUserAgent, "UofSdk-Net");
                                     })
                .ConfigurePrimaryHttpMessageHandler(() => _stubMessageHandler);
        var serviceProvider = services.BuildServiceProvider();
        _sdkHttpClient = new SdkHttpClient(serviceProvider.GetRequiredService<IHttpClientFactory>(), HttpClientDefaultName);

        _logHttpDataFetcher = new LogHttpDataFetcher(_sdkHttpClient, new Deserializer<response>(), new NullLogger<LogHttpDataFetcher>());
    }

    [Fact]
    public void SdkHttpClientWhenConstructedThenSetTimeout()
    {
        _sdkHttpClient.Timeout.TotalSeconds.Should().Be(5);
    }

    [Fact]
    public void SdkHttpClientWhenWithoutHeadersThenThrow()
    {
        var services = new ServiceCollection();
        services.AddHttpClient(HttpClientDefaultName)
                .ConfigureHttpClient(configureClient =>
                                     {
                                         configureClient.Timeout = TimeSpan.FromSeconds(5);
                                         configureClient.DefaultRequestHeaders.Clear();
                                     })
                .ConfigurePrimaryHttpMessageHandler(() => _stubMessageHandler);
        var serviceProvider = services.BuildServiceProvider();

        Assert.Throws<InvalidOperationException>(() => new SdkHttpClient(serviceProvider.GetRequiredService<IHttpClientFactory>(), HttpClientDefaultName));
    }

    [Fact]
    public void SdkHttpClientWhenWithoutAccessTokenThenThrow()
    {
        var services = new ServiceCollection();
        services.AddHttpClient(HttpClientDefaultName)
                .ConfigureHttpClient(configureClient =>
                                     {
                                         configureClient.Timeout = TimeSpan.FromSeconds(5);
                                         configureClient.DefaultRequestHeaders.Add(UofSdkBootstrap.HttpClientDefaultRequestHeaderForUserAgent, "UofSdk-Net");
                                     })
                .ConfigurePrimaryHttpMessageHandler(() => _stubMessageHandler);
        var serviceProvider = services.BuildServiceProvider();

        Assert.Throws<InvalidOperationException>(() => new SdkHttpClient(serviceProvider.GetRequiredService<IHttpClientFactory>(), HttpClientDefaultName));
    }

    [Fact]
    public void SdkHttpClientWhenWithoutUserAgentThenThrow()
    {
        var services = new ServiceCollection();
        services.AddHttpClient(HttpClientDefaultName)
                .ConfigureHttpClient(configureClient =>
                                     {
                                         configureClient.Timeout = TimeSpan.FromSeconds(5);
                                         configureClient.DefaultRequestHeaders.Add(UofSdkBootstrap.HttpClientDefaultRequestHeaderForAccessToken, "aaa");
                                     })
                .ConfigurePrimaryHttpMessageHandler(() => _stubMessageHandler);
        var serviceProvider = services.BuildServiceProvider();

        Assert.Throws<InvalidOperationException>(() => new SdkHttpClient(serviceProvider.GetRequiredService<IHttpClientFactory>(), HttpClientDefaultName));
    }

    [Fact]
    public void SdkHttpClientWhenWithLowCaseUserAgentThenDoNotThrow()
    {
        var services = new ServiceCollection();
        services.AddHttpClient(HttpClientDefaultName)
                .ConfigureHttpClient(configureClient =>
                                     {
                                         configureClient.Timeout = TimeSpan.FromSeconds(5);
                                         configureClient.DefaultRequestHeaders.Clear();
                                         configureClient.DefaultRequestHeaders.Add(UofSdkBootstrap.HttpClientDefaultRequestHeaderForAccessToken, "aaa");
                                         configureClient.DefaultRequestHeaders.Add("user-agent", "UofSdk-Net");
                                     })
                .ConfigurePrimaryHttpMessageHandler(() => _stubMessageHandler);
        var serviceProvider = services.BuildServiceProvider();

        var sdkHttpClient = new SdkHttpClient(serviceProvider.GetRequiredService<IHttpClientFactory>(), HttpClientDefaultName);

        sdkHttpClient.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDataAsyncWhenNormalUri()
    {
        // in logRest file there should be result for this call
        var result = await _logHttpDataFetcher.GetDataAsync(_getUri);

        Assert.NotNull(result);
        Assert.True(result.CanRead);

        var s = await new StreamReader(result).ReadToEndAsync();
        Assert.True(!string.IsNullOrEmpty(s));
    }

    [Fact]
    public void GetDataWhenNormalUriOnDebugLog()
    {
        var mockLocker = new Mock<ILogger>();
        mockLocker.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
        var logHttpDataFetcher = new LogHttpDataFetcher(_sdkHttpClient, new Deserializer<response>(), mockLocker.Object);

        var result = logHttpDataFetcher.GetData(_getUri);

        Assert.NotNull(result);
        Assert.True(result.CanRead);

        var s = new StreamReader(result).ReadToEnd();
        Assert.True(!string.IsNullOrEmpty(s));
    }

    [Fact]
    public async Task GetDataAsyncWhenWrongUrlThenReturnsBadGateway()
    {
        _stubMessageHandler.SetWantedResponse(new HttpResponseMessage(HttpStatusCode.BadGateway));
        Stream result = null;

        var e = await Assert.ThrowsAsync<CommunicationException>(async () => result = await _logHttpDataFetcher.GetDataAsync(_badUri));

        Assert.Null(result);
        Assert.IsType<CommunicationException>(e);
        if (e.InnerException != null)
        {
            Assert.IsType<DeserializationException>(e.InnerException);
        }
    }

    [Fact]
    public async Task GetDataAsyncWhenWrongUrlThenThrowsHttpRequestException()
    {
        _stubMessageHandler.SetWantedResponse(new HttpRequestException("any-message", new SocketException(), HttpStatusCode.BadGateway));
        Stream result = null;

        var e = await Assert.ThrowsAsync<CommunicationException>(async () => result = await _logHttpDataFetcher.GetDataAsync(_badUri));

        Assert.Null(result);
        Assert.IsType<CommunicationException>(e);
        if (e.InnerException != null)
        {
            Assert.IsType<HttpRequestException>(e.InnerException);
        }
    }

    [Fact]
    public async Task GetDataAsyncWhenWrongUrlThenThrowsHttpRequestExceptionWithNotFound()
    {
        _stubMessageHandler.SetWantedResponse(new HttpRequestException("NotFound any-message", new SocketException(), HttpStatusCode.NotFound));
        Stream result = null;

        var e = await Assert.ThrowsAsync<CommunicationException>(async () => result = await _logHttpDataFetcher.GetDataAsync(_badUri));

        Assert.Null(result);
        Assert.IsType<CommunicationException>(e);
        if (e.InnerException != null)
        {
            Assert.IsType<HttpRequestException>(e.InnerException);
        }
    }

    [Fact]
    public async Task GetDataAsyncWhenWrongUrlThenThrowsSocketException()
    {
        _stubMessageHandler.SetWantedResponse(new SocketException());
        Stream result = null;

        var e = await Assert.ThrowsAsync<CommunicationException>(async () => result = await _logHttpDataFetcher.GetDataAsync(_badUri));

        Assert.Null(result);
        Assert.IsType<CommunicationException>(e);
        if (e.InnerException != null)
        {
            Assert.IsType<SocketException>(e.InnerException);
        }
    }

    [Fact]
    public async Task PostDataWhenNormalUri()
    {
        var result = await _logHttpDataFetcher.PostDataAsync(_postUri);

        Assert.NotNull(result);
        Assert.True(result.IsSuccessStatusCode);
    }

    [Fact]
    public async Task PostData_DebugLog()
    {
        var mockLocker = new Mock<ILogger>();
        mockLocker.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
        var logHttpDataFetcher = new LogHttpDataFetcher(_sdkHttpClient, new Deserializer<response>(), mockLocker.Object);

        var result = await logHttpDataFetcher.PostDataAsync(_getUri);

        Assert.NotNull(result);
        Assert.True(result.IsSuccessStatusCode);
    }

    [Fact]
    public async Task PostDataWhenDebugLogWithContent()
    {
        var mockLocker = new Mock<ILogger>();
        mockLocker.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
        var logHttpDataFetcher = new LogHttpDataFetcher(_sdkHttpClient, new Deserializer<response>(), mockLocker.Object);

        var result = await logHttpDataFetcher.PostDataAsync(_getUri, new StringContent("test string"));

        Assert.NotNull(result);
        Assert.True(result.IsSuccessStatusCode);
    }

    [Fact]
    public async Task PostDataWhenDebugLogWithContentThenFails()
    {
        _stubMessageHandler.SetWantedResponse(new HttpResponseMessage(HttpStatusCode.BadGateway));
        var mockLocker = new Mock<ILogger>();
        mockLocker.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
        var logHttpDataFetcher = new LogHttpDataFetcher(_sdkHttpClient, new Deserializer<response>(), mockLocker.Object);

        var result = await logHttpDataFetcher.PostDataAsync(_getUri, new StringContent("test string"));

        Assert.NotNull(result);
        Assert.False(result.IsSuccessStatusCode);
    }

    [Fact]
    public async Task PostDataWhenDebugLogWithContentAndNullUrlThenFails()
    {
        _stubMessageHandler.SetWantedResponse(new HttpResponseMessage(HttpStatusCode.BadGateway));
        var mockLocker = new Mock<ILogger>();
        mockLocker.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
        var logHttpDataFetcher = new LogHttpDataFetcher(_sdkHttpClient, new Deserializer<response>(), mockLocker.Object);

        HttpResponseMessage result = null;
        _ = await Assert.ThrowsAsync<NullReferenceException>(async () => result = await logHttpDataFetcher.PostDataAsync(null, new StringContent("test string")));

        Assert.Null(result);
    }

    [Fact]
    [SuppressMessage("ReSharper", "UseCollectionExpression", Justification = "Throws formatting issue in pipeline")]
    public async Task PostDataWhenEmptyResponseContent()
    {
        _stubMessageHandler.SetWantedResponse(new HttpResponseMessage(HttpStatusCode.Accepted)
        {
            Content = new ByteArrayContent(Array.Empty<byte>())
        });
        var result = await _logHttpDataFetcher.PostDataAsync(_postUri);

        Assert.NotNull(result);
        Assert.True(result.IsSuccessStatusCode);
    }

    [Fact]
    public async Task PostDataAsyncTestWithWrongUrl()
    {
        _stubMessageHandler.SetWantedResponse(new HttpRequestException("any-message", new SocketException(), HttpStatusCode.BadGateway));
        HttpResponseMessage result = null;

        var ex = await Assert.ThrowsAsync<CommunicationException>(async () => result = await _logHttpDataFetcher.PostDataAsync(_badUri));

        Assert.Null(result);
        Assert.IsType<CommunicationException>(ex);
        if (ex.InnerException != null)
        {
            Assert.IsType<HttpRequestException>(ex.InnerException);
        }
    }

    [Fact]
    public async Task PostDataAsyncWhenWithContent()
    {
        var result = await _logHttpDataFetcher.PostDataAsync(_postUri, new StringContent("test string"));

        Assert.NotNull(result);
        Assert.True(result.IsSuccessStatusCode);
    }
}
