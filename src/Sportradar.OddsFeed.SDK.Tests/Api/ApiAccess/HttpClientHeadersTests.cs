// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Authentication;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Api.ApiAccess;

[Collection(NonParallelCollectionFixture.NonParallelTestCollection)]
public class HttpClientHeadersTests
{
    private const string TraceIdHeaderName = HttpApiConstants.TraceIdHeaderName;
    private const string AnyUrlPath = "/any/path";

    private readonly IServiceCollection _serviceCollection;
    private readonly Mock<IRequestDecorator> _requestDecoratorMock;
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly WireMockServer _wireMockServer;

    public HttpClientHeadersTests(ITestOutputHelper outputHelper)
    {
        _testOutputHelper = outputHelper;
        _wireMockServer = WireMockServer.Start();
        _serviceCollection = new ServiceCollection();
        _loggerFactoryMock = new Mock<ILoggerFactory>();

        var uofConfigMock = new Mock<IUofConfiguration>();
        ConfigureCacheConfig(uofConfigMock);
        ConfigureApiConfigTo2SecTimeout(uofConfigMock, _wireMockServer.Url);
        ConfigureAuthConfig(uofConfigMock, _wireMockServer.Url, _wireMockServer.Port);

        _serviceCollection.AddSingleton(_ => _loggerFactoryMock.Object);
        _serviceCollection.AddUofSdkServices(uofConfigMock.Object);

        // replace production implementation with mocks
        _requestDecoratorMock = new Mock<IRequestDecorator>();
        _serviceCollection.AddSingleton(_ => _requestDecoratorMock.Object);

        var authTokenCacheMock = new Mock<IAuthenticationTokenCache>();
        authTokenCacheMock.Setup(c => c.GetTokenForApi()).ReturnsAsync("any-valid-jwt-token");
        _serviceCollection.AddSingleton(_ => authTokenCacheMock.Object);
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Critical)]
    public async Task HttpFastFailingClientDecoratesEachRequestWithTraceIdWhenCalledGetDataAsync(LogLevel logLevel)
    {
        var requestHeaderId = Guid.NewGuid();
        SetupLoggerFactory(logLevel);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());
        SetupWireMockServerForGetAnyRequest();

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcherFastFailing>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        _ = await logHttpFetcher.GetDataAsync(new Uri($"{_wireMockServer.Url}{AnyUrlPath}"));

        _wireMockServer.LogEntries.ShouldBeOfSize(1);
        _wireMockServer.LogEntries.ShouldContain(e => e.RequestMessage.Path.Contains(AnyUrlPath));

        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));

        StopWireMockServer();
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Critical)]
    public void HttpFastFailingClientDecoratesEachRequestWithTraceIdWhenCalledGetData(LogLevel logLevel)
    {
        var requestHeaderId = Guid.NewGuid();
        SetupLoggerFactory(logLevel);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());
        SetupWireMockServerForGetAnyRequest();

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcherFastFailing>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        _ = logHttpFetcher.GetData(new Uri($"{_wireMockServer.Url}{AnyUrlPath}"));

        _wireMockServer.LogEntries.ShouldBeOfSize(1);
        _wireMockServer.LogEntries.ShouldContain(e => e.RequestMessage.Path.Contains(AnyUrlPath));

        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));

        StopWireMockServer();
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Critical)]
    public async Task HttpNormalClientDecoratesEachRequestWithTraceIdWhenCalledGetDataAsync(LogLevel logLevel)
    {
        var requestHeaderId = Guid.NewGuid();
        SetupLoggerFactory(logLevel);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());
        SetupWireMockServerForGetAnyRequest();

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcher>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        _ = await logHttpFetcher.GetDataAsync(new Uri($"{_wireMockServer.Url}{AnyUrlPath}"));

        _wireMockServer.LogEntries.ShouldBeOfSize(1);
        _wireMockServer.LogEntries.ShouldContain(e => e.RequestMessage.Path.Contains(AnyUrlPath));

        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));

        StopWireMockServer();
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Critical)]
    public void HttpNormalClientDecoratesEachRequestWithTraceIdWhenCalledGetData(LogLevel logLevel)
    {
        var requestHeaderId = Guid.NewGuid();
        SetupLoggerFactory(logLevel);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());
        SetupWireMockServerForGetAnyRequest();

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcher>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        _ = logHttpFetcher.GetData(new Uri($"{_wireMockServer.Url}{AnyUrlPath}"));

        _wireMockServer.LogEntries.ShouldBeOfSize(1);
        _wireMockServer.LogEntries.ShouldContain(e => e.RequestMessage.Path.Contains(AnyUrlPath));

        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));

        StopWireMockServer();
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Critical)]
    public async Task HttpRecoveryClientDecoratesEachRequestWithTraceIdWhenCalledGetDataAsync(LogLevel logLevel)
    {
        var requestHeaderId = Guid.NewGuid();
        SetupLoggerFactory(logLevel);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());
        SetupWireMockServerForGetAnyRequest();

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcherRecovery>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        _ = await logHttpFetcher.GetDataAsync(new Uri($"{_wireMockServer.Url}{AnyUrlPath}"));

        _wireMockServer.LogEntries.ShouldBeOfSize(1);
        _wireMockServer.LogEntries.ShouldContain(e => e.RequestMessage.Path.Contains(AnyUrlPath));

        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));

        StopWireMockServer();
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Critical)]
    public void HttpRecoveryClientDecoratesEachRequestWithTraceIdWhenCalledGetData(LogLevel logLevel)
    {
        var requestHeaderId = Guid.NewGuid();
        SetupLoggerFactory(logLevel);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());
        SetupWireMockServerForGetAnyRequest();

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcherRecovery>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        _ = logHttpFetcher.GetData(new Uri($"{_wireMockServer.Url}{AnyUrlPath}"));

        _wireMockServer.LogEntries.ShouldBeOfSize(1);
        _wireMockServer.LogEntries.ShouldContain(e => e.RequestMessage.Path.Contains(AnyUrlPath));

        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));

        StopWireMockServer();
    }

    [Fact]
    public async Task HttpFastFailingRequestHasTraceIdWhenExceptionIsThrownInGetDataAsync()
    {
        var requestHeaderId = Guid.NewGuid();
        SetupLoggerFactory(LogLevel.Trace);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());
        SetupWireMockServerToReturnNotFoundOnGetRequest();

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcherFastFailing>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        var exception = await Should.ThrowAsync<CommunicationException>(async () => await logHttpFetcher.GetDataAsync(new Uri($"{_wireMockServer.Url}{AnyUrlPath}")));

        exception.ShouldNotBeNull();

        _wireMockServer.LogEntries.ShouldBeOfSize(1);
        _wireMockServer.LogEntries.ShouldContain(e => e.RequestMessage.Path.Contains(AnyUrlPath));

        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));

        StopWireMockServer();
    }

    [Fact]
    public async Task HttpFastFailingRequestHasTraceIdWhenNotFoundIsReturnedByGetDataAsync()
    {
        var requestHeaderId = Guid.NewGuid();
        SetupLoggerFactory(LogLevel.Trace);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());
        SetupWireMockServerToReturnNotFoundOnGetRequest();

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcherFastFailing>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        var exception = await Should.ThrowAsync<CommunicationException>(async () => await logHttpFetcher.GetDataAsync(new Uri($"{_wireMockServer.Url}{AnyUrlPath}")));

        exception.ShouldNotBeNull();

        _wireMockServer.LogEntries.ShouldBeOfSize(1);
        _wireMockServer.LogEntries.ShouldContain(e => e.RequestMessage.Path.Contains(AnyUrlPath));

        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));

        StopWireMockServer();
    }

    [Fact]
    public async Task HttpFastFailingRequestHasTraceIdWhenRequestTriggeredTimeoutInGetDataAsync()
    {
        var requestHeaderId = Guid.NewGuid();
        SetupLoggerFactory(LogLevel.Trace);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());
        SetupWireMockServerWithDelayOnAnyRequest();

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcherFastFailing>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        var exception = await Should.ThrowAsync<CommunicationException>(async () => await logHttpFetcher.GetDataAsync(new Uri($"{_wireMockServer.Url}{AnyUrlPath}")));

        TestExecutionHelper.WaitToComplete(() => _wireMockServer.LogEntries.ShouldBeOfSize(1));
        ValidateRequestLogEntriesHasTraceId(requestHeaderId);
        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));
        exception.ShouldNotBeNull();
        exception.InnerException.ShouldNotBeNull();
        exception.InnerException.ShouldBeOfType<TaskCanceledException>();

        StopWireMockServer();
    }

    [Fact]
    public async Task HttpRequestHasTraceIdWhenRequestTriggeredTimeoutInGetDataAsync()
    {
        var requestHeaderId = Guid.NewGuid();
        SetupLoggerFactory(LogLevel.Trace);
        SetupWireMockServerWithDelayOnAnyRequest();
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcher>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        var exception = await Should.ThrowAsync<CommunicationException>(async () => await logHttpFetcher.GetDataAsync(new Uri($"{_wireMockServer.Url}{AnyUrlPath}")));

        TestExecutionHelper.WaitToComplete(() => _wireMockServer.LogEntries.ShouldBeOfSize(1));
        ValidateRequestLogEntriesHasTraceId(requestHeaderId);
        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));
        exception.ShouldNotBeNull();
        exception.InnerException.ShouldNotBeNull();
        exception.InnerException.ShouldBeOfType<TaskCanceledException>();

        StopWireMockServer();
    }

    [Fact]
    public async Task HttpRecoveryRequestHasTraceIdWhenRequestTriggeredTimeoutInPostDataAsync()
    {
        var requestHeaderId = Guid.NewGuid();
        SetupLoggerFactory(LogLevel.Trace);
        SetupWireMockServerWithDelayOnAnyRequest();
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcherRecovery>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        var exception = await Should.ThrowAsync<CommunicationException>(async () => await logHttpFetcher.PostDataAsync(new Uri($"{_wireMockServer.Url}{AnyUrlPath}")));

        TestExecutionHelper.WaitToComplete(() => _wireMockServer.LogEntries.ShouldBeOfSize(1));
        ValidateRequestLogEntriesHasTraceId(requestHeaderId);
        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));
        exception.ShouldNotBeNull();
        exception.InnerException.ShouldNotBeNull();
        exception.InnerException.ShouldBeOfType<TaskCanceledException>();

        StopWireMockServer();
    }

    [Fact]
    public async Task HttpRequestHasTraceIdWhenNotFoundIsReturnedByGetDataAsync()
    {
        var requestHeaderId = Guid.NewGuid();
        SetupLoggerFactory(LogLevel.Trace);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());
        SetupWireMockServerToReturnNotFoundOnGetRequest();

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcher>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        var exception = await Should.ThrowAsync<CommunicationException>(async () => await logHttpFetcher.GetDataAsync(new Uri($"{_wireMockServer.Url}{AnyUrlPath}")));

        exception.ShouldNotBeNull();

        _wireMockServer.LogEntries.ShouldBeOfSize(1);
        _wireMockServer.LogEntries.ShouldContain(e => e.RequestMessage.Path.Contains(AnyUrlPath));
        ValidateRequestLogEntriesHasTraceId(requestHeaderId);

        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));

        StopWireMockServer();
    }

    [Fact]
    public async Task HttpRecoveryRequestHasTraceIdWhenNotFoundIsReturnedByGetDataAsync()
    {
        var requestHeaderId = Guid.NewGuid();
        SetupLoggerFactory(LogLevel.Trace);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());
        SetupWireMockServerToReturnNotFoundOnGetRequest();

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcherRecovery>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        var exception = await Should.ThrowAsync<CommunicationException>(async () => await logHttpFetcher.GetDataAsync(new Uri($"{_wireMockServer.Url}{AnyUrlPath}")));

        exception.ShouldNotBeNull();

        _wireMockServer.LogEntries.ShouldBeOfSize(1);
        _wireMockServer.LogEntries.ShouldContain(e => e.RequestMessage.Path.Contains(AnyUrlPath));
        ValidateRequestLogEntriesHasTraceId(requestHeaderId);

        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));

        StopWireMockServer();
    }

    [Fact]
    public void HttpFastFailingRequestHasTraceIdWhenExceptionIsThrownInGetData()
    {
        var requestHeaderId = Guid.NewGuid();
        SetupLoggerFactory(LogLevel.Trace);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());
        SetupWireMockServerToReturnBadRequestOnGetRequest();

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcherFastFailing>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        var exception = Should.Throw<CommunicationException>(() => logHttpFetcher.GetData(new Uri($"{_wireMockServer.Url}{AnyUrlPath}")));

        exception.ShouldNotBeNull();

        _wireMockServer.LogEntries.ShouldBeOfSize(1);
        _wireMockServer.LogEntries.ShouldContain(e => e.RequestMessage.Path.Contains(AnyUrlPath));
        ValidateRequestLogEntriesHasTraceId(requestHeaderId);

        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));

        StopWireMockServer();
    }

    [Fact]
    public async Task HttpNormalRequestHasTraceIdWhenExceptionIsThrownInGetDataAsync()
    {
        var requestHeaderId = Guid.NewGuid();
        SetupLoggerFactory(LogLevel.Trace);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());
        SetupWireMockServerToReturnBadRequestOnGetRequest();

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcher>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        var exception = await Should.ThrowAsync<CommunicationException>(async () => await logHttpFetcher.GetDataAsync(new Uri($"{_wireMockServer.Url}{AnyUrlPath}")));

        exception.ShouldNotBeNull();

        _wireMockServer.LogEntries.ShouldBeOfSize(1);
        _wireMockServer.LogEntries.ShouldContain(e => e.RequestMessage.Path.Contains(AnyUrlPath));
        ValidateRequestLogEntriesHasTraceId(requestHeaderId);

        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));

        StopWireMockServer();
    }

    [Fact]
    public void HttpNormalRequestHasTraceIdWhenExceptionIsThrownInGetData()
    {
        var requestHeaderId = Guid.NewGuid();
        SetupLoggerFactory(LogLevel.Trace);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());
        SetupWireMockServerToReturnBadRequestOnGetRequest();

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcher>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        var exception = Should.Throw<CommunicationException>(() => logHttpFetcher.GetData(new Uri($"{_wireMockServer.Url}{AnyUrlPath}")));

        exception.ShouldNotBeNull();

        _wireMockServer.LogEntries.ShouldBeOfSize(1);
        _wireMockServer.LogEntries.ShouldContain(e => e.RequestMessage.Path.Contains(AnyUrlPath));
        ValidateRequestLogEntriesHasTraceId(requestHeaderId);

        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));

        StopWireMockServer();
    }

    [Fact]
    public async Task HttpRecoveryRequestHasTraceIdWhenExceptionIsThrownInGetDataAsync()
    {
        var requestHeaderId = Guid.NewGuid();
        SetupLoggerFactory(LogLevel.Trace);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());
        SetupWireMockServerToReturnBadRequestOnGetRequest();

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcherRecovery>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        var exception = await Should.ThrowAsync<CommunicationException>(async () => await logHttpFetcher.GetDataAsync(new Uri($"{_wireMockServer.Url}{AnyUrlPath}")));

        exception.ShouldNotBeNull();

        _wireMockServer.LogEntries.ShouldBeOfSize(1);
        _wireMockServer.LogEntries.ShouldContain(e => e.RequestMessage.Path.Contains(AnyUrlPath));
        ValidateRequestLogEntriesHasTraceId(requestHeaderId);

        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));

        StopWireMockServer();
    }

    [Fact]
    public void HttpRecoveryRequestHasTraceIdWhenExceptionIsThrownInGetData()
    {
        var requestHeaderId = Guid.NewGuid();
        SetupLoggerFactory(LogLevel.Trace);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());
        SetupWireMockServerToReturnNotFoundOnGetRequest();

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcherRecovery>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        var exception = Should.Throw<CommunicationException>(() => logHttpFetcher.GetData(new Uri($"{_wireMockServer.Url}{AnyUrlPath}")));

        exception.ShouldNotBeNull();

        _wireMockServer.LogEntries.ShouldBeOfSize(1);
        _wireMockServer.LogEntries.ShouldContain(e => e.RequestMessage.Path.Contains(AnyUrlPath));
        ValidateRequestLogEntriesHasTraceId(requestHeaderId);

        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));

        StopWireMockServer();
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Critical)]
    public async Task HttpFastFailingClientDecoratesEachRequestWithTraceIdWhenCallPostAsync(LogLevel logLevel)
    {
        var requestHeaderId = Guid.NewGuid();
        SetupLoggerFactory(logLevel);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());
        SetupWireMockServerForPostAnyRequest();

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcherFastFailing>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        var response = await logHttpFetcher.PostDataAsync(new Uri($"{_wireMockServer.Url}{AnyUrlPath}"), new StringContent(string.Empty));

        ValidateRequestLogEntriesHasTraceId(requestHeaderId);
        response.RequestMessage.ShouldNotBeNull();
        response.RequestMessage.Headers.ShouldContain(h => string.Equals(h.Key, HttpApiConstants.TraceIdHeaderName, StringComparison.OrdinalIgnoreCase));
        response.RequestMessage.Headers.GetValues(HttpApiConstants.TraceIdHeaderName).First().ShouldBe(requestHeaderId.ToString());
        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));

        StopWireMockServer();
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Critical)]
    public async Task HttpNormalClientDecoratesEachRequestWithTraceIdWhenCallPostAsync(LogLevel logLevel)
    {
        var requestHeaderId = Guid.NewGuid();
        SetupLoggerFactory(logLevel);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());
        SetupWireMockServerForPostAnyRequest();

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcher>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        var response = await logHttpFetcher.PostDataAsync(new Uri($"{_wireMockServer.Url}{AnyUrlPath}"), new StringContent(string.Empty));

        _wireMockServer.LogEntries.ShouldBeOfSize(1);
        ValidateRequestLogEntriesHasTraceId(requestHeaderId);
        response.RequestMessage.ShouldNotBeNull();
        response.RequestMessage.Headers.ShouldContain(h => string.Equals(h.Key, HttpApiConstants.TraceIdHeaderName, StringComparison.OrdinalIgnoreCase));
        response.RequestMessage.Headers.GetValues(HttpApiConstants.TraceIdHeaderName).First().ShouldBe(requestHeaderId.ToString());
        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));

        StopWireMockServer();
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Critical)]
    public async Task HttpRecoveryClientDecoratesEachRequestWithTraceIdWhenCallPostAsync(LogLevel logLevel)
    {
        var requestHeaderId = Guid.NewGuid();
        SetupLoggerFactory(logLevel);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());
        SetupWireMockServerForPostAnyRequest();

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcherRecovery>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        var response = await logHttpFetcher.PostDataAsync(new Uri($"{_wireMockServer.Url}{AnyUrlPath}"), new StringContent(string.Empty));

        _wireMockServer.LogEntries.ShouldBeOfSize(1);
        ValidateRequestLogEntriesHasTraceId(requestHeaderId);
        response.RequestMessage.ShouldNotBeNull();
        response.RequestMessage.Headers.ShouldContain(h => string.Equals(h.Key, HttpApiConstants.TraceIdHeaderName, StringComparison.OrdinalIgnoreCase));
        response.RequestMessage.Headers.GetValues(HttpApiConstants.TraceIdHeaderName).First().ShouldBe(requestHeaderId.ToString());
        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));

        StopWireMockServer();
    }

    [Fact]
    public void GetTraceIdWhenNullMessageThenReturnsEmpty()
    {
        var traceId = ((HttpRequestMessage)null).GetTraceId();

        traceId.ShouldBeEmpty();
    }

    [Fact]
    public void GetTraceIdWhenNoHeadersThenReturnsEmpty()
    {
        var requestMessage = new HttpRequestMessage();

        var traceId = requestMessage.GetTraceId();

        traceId.ShouldBeEmpty();
        requestMessage.Headers.ShouldNotBeNull();
        requestMessage.Headers.ShouldBeEmpty();
    }

    [Fact]
    public void GetTraceIdWhenNoTraceHeaderThenReturnsEmpty()
    {
        var requestMessage = new HttpRequestMessage();
        requestMessage.Headers.Add("any-header", "any-value");

        var traceId = requestMessage.GetTraceId();

        traceId.ShouldBeEmpty();
        requestMessage.Headers.ShouldNotBeNull();
        requestMessage.Headers.ShouldNotBeEmpty();
    }

    [Fact]
    public void GetTraceIdWhenHasTraceHeaderThenReturnsTraceId()
    {
        const string traceGuid = "any-value";
        var requestMessage = new HttpRequestMessage();
        requestMessage.Headers.Add(HttpApiConstants.TraceIdHeaderName, traceGuid);

        var traceId = requestMessage.GetTraceId();

        traceId.ShouldBe(traceGuid);
    }

    [Fact]
    public void GetTraceIdWhenNoTraceValueInHeaderThenReturnsEmpty()
    {
        var requestMessage = new HttpRequestMessage();
        requestMessage.Headers.Add(HttpApiConstants.TraceIdHeaderName, string.Empty);

        var traceId = requestMessage.GetTraceId();

        traceId.ShouldBeEmpty();
        requestMessage.Headers.ShouldNotBeNull();
        requestMessage.Headers.ShouldNotBeEmpty();
    }

    private void SetupLoggerFactory(LogLevel logLevel)
    {
        _loggerFactoryMock.Setup(lf => lf.CreateLogger(It.IsAny<string>())).Returns(new XUnitLogger(It.IsAny<string>(), _testOutputHelper, logLevel));
    }

    private void SetupRequestDecoratorWithHeader(string headerName, string headerValue)
    {
        _requestDecoratorMock.Setup(d => d.Decorate(It.IsAny<HttpRequestMessage>()))
                             .Callback<HttpRequestMessage>(r =>
                                                           {
                                                               r.Headers.Add(headerName, headerValue);
                                                           });
    }

    private static void ConfigureApiConfigTo2SecTimeout(Mock<IUofConfiguration> uofConfigMock, string wiremockUrl)
    {
        uofConfigMock.Setup(config => config.Api)
                     .Returns(new UofApiConfiguration
                     {
                         MaxConnectionsPerServer = 100,
                         Host = GetDomainFromUrl(wiremockUrl),
                         UseSsl = false,
                         HttpClientTimeout = TimeSpan.FromSeconds(2),
                         HttpClientRecoveryTimeout = TimeSpan.FromSeconds(2),
                         HttpClientFastFailingTimeout = TimeSpan.FromSeconds(2)
                     });
    }

    private static void ConfigureCacheConfig(Mock<IUofConfiguration> uofConfigMock)
    {
        uofConfigMock.Setup(config => config.Cache)
                     .Returns(new UofCacheConfiguration
                     {
                         SportEventCacheTimeout = TimeSpan.FromMinutes(1),
                         ProfileCacheTimeout = TimeSpan.FromMinutes(1),
                         SportEventStatusCacheTimeout = TimeSpan.FromMinutes(1),
                         VariantMarketDescriptionCacheTimeout = TimeSpan.FromMinutes(1),
                         IgnoreBetPalTimelineSportEventStatus = true,
                         IgnoreBetPalTimelineSportEventStatusCacheTimeout = TimeSpan.FromMinutes(1)
                     });
    }

    private static void ConfigureAuthConfig(Mock<IUofConfiguration> uofConfigMock, string wiremockUrl, int wiremockPort)
    {
        uofConfigMock.Setup(s => s.AccessToken).Returns(TestConsts.AnyAccessToken);

        AsymmetricSecurityKey testPrivateKey = new RsaSecurityKey(RSA.Create(2056));
        var privateKeyJwt = new PrivateKeyJwt("signing-key-id", "client-id", testPrivateKey);
        privateKeyJwt.SetHost(GetDomainFromUrl(wiremockUrl));
        privateKeyJwt.SetUseSsl(false);
        privateKeyJwt.SetPort(wiremockPort);

        uofConfigMock.Setup(config => config.Authentication).Returns(privateKeyJwt);
    }

    private static string GetDomainFromUrl(string url)
    {
        var uri = new Uri(url);
        return uri.DnsSafeHost;
    }

    private void SetupWireMockServerForGetAnyRequest()
    {
        _wireMockServer.Given(Request.Create().UsingGet()).RespondWith(Response.Create().WithBody("api-request-received").WithStatusCode(StatusCodes.Status200OK));
    }

    private void SetupWireMockServerForPostAnyRequest()
    {
        _wireMockServer.Given(Request.Create().UsingPost()).RespondWith(Response.Create().WithBody("api-request-received").WithStatusCode(StatusCodes.Status200OK));
    }

    private void SetupWireMockServerToReturnNotFoundOnGetRequest()
    {
        _wireMockServer.Given(Request.Create().UsingGet()).RespondWith(Response.Create().WithBody("api-request-received").WithStatusCode(StatusCodes.Status404NotFound));
    }

    private void SetupWireMockServerToReturnBadRequestOnGetRequest()
    {
        _wireMockServer.Given(Request.Create().UsingGet()).RespondWith(Response.Create().WithBody("api-request-received").WithStatusCode(StatusCodes.Status400BadRequest));
    }

    private void SetupWireMockServerWithDelayOnAnyRequest()
    {
        _wireMockServer.Given(Request.Create().UsingGet()).RespondWith(Response.Create().WithDelay(2200).WithBody("api-request-received").WithStatusCode(StatusCodes.Status200OK));
        _wireMockServer.Given(Request.Create().UsingPost()).RespondWith(Response.Create().WithDelay(2200).WithBody("api-request-received").WithStatusCode(StatusCodes.Status200OK));
    }

    private void ValidateRequestLogEntriesHasTraceId(Guid requestHeaderId)
    {
        _wireMockServer.LogEntries.ShouldContain(e => e.RequestMessage.Path.Contains(AnyUrlPath));
        var logEntry = _wireMockServer.LogEntries.FirstOrDefault(f => f.RequestMessage.Headers.ContainsKey(HttpApiConstants.TraceIdHeaderName));
        logEntry.ShouldNotBeNull();
        logEntry.RequestMessage.Headers.ShouldNotBeNull();
        logEntry.RequestMessage.Headers.ShouldContain(h => h.Key.Equals(HttpApiConstants.TraceIdHeaderName, StringComparison.OrdinalIgnoreCase));
        logEntry.RequestMessage.Headers[HttpApiConstants.TraceIdHeaderName].First().ShouldBe(requestHeaderId.ToString());
    }

    private void StopWireMockServer()
    {
        _wireMockServer.Stop();
    }
}
