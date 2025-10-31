// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Handlers;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Api.ApiAccess;

public class HttpClientHeadersTests
{
    private const string TraceIdHeaderName = HttpApiConstants.TraceIdHeaderName;

    private readonly IServiceCollection _serviceCollection;
    private readonly Mock<IRequestDecorator> _requestDecoratorMock;
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly Mock<IRequestHeaderInspector> _requestHeaderInspectorMock;
    private readonly ITestOutputHelper _testOutputHelper;

    public HttpClientHeadersTests(ITestOutputHelper outputHelper)
    {
        _testOutputHelper = outputHelper;
        _serviceCollection = new ServiceCollection();
        _requestDecoratorMock = new Mock<IRequestDecorator>();
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _requestHeaderInspectorMock = new Mock<IRequestHeaderInspector>();

        var uofConfigMock = new Mock<IUofConfiguration>();
        ConfigureCacheConfig(uofConfigMock);
        ConfigureApiConfig(uofConfigMock);

        _serviceCollection.AddSingleton(_ => _loggerFactoryMock.Object);
        _serviceCollection.AddUofSdkServices(uofConfigMock.Object);
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Critical)]
    public async Task HttpFastFailingClientDecoratesEachRequestWithTraceIdWhenCalledGetDataAsync(LogLevel logLevel)
    {
        var requestHeaderId = Guid.Parse("34965490-d4ab-4374-a8f9-b2a3740c3e84");
        const string httpClientName = UofSdkBootstrap.HttpClientNameForFastFailing;

        SetupLoggerFactory(logLevel);
        SetupHttpClientWithResponseOk(httpClientName);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcherFastFailing>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        _ = await logHttpFetcher.GetDataAsync(new Uri("http://localhost/"));

        _requestHeaderInspectorMock.Verify(d => d.VerifyRequestHeader(requestHeaderId.ToString()), Times.Once);
        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Critical)]
    public void HttpFastFailingClientDecoratesEachRequestWithTraceIdWhenCalledGetData(LogLevel logLevel)
    {
        var requestHeaderId = Guid.Parse("34965490-d4ab-4374-a8f9-b2a3740c3e84");
        const string httpClientName = UofSdkBootstrap.HttpClientNameForFastFailing;

        SetupLoggerFactory(logLevel);
        SetupHttpClientWithResponseOk(httpClientName);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcherFastFailing>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        _ = logHttpFetcher.GetData(new Uri("http://localhost/"));

        _requestHeaderInspectorMock.Verify(d => d.VerifyRequestHeader(requestHeaderId.ToString()), Times.Once);
        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Critical)]
    public async Task HttpNormalClientDecoratesEachRequestWithTraceIdWhenCalledGetDataAsync(LogLevel logLevel)
    {
        var requestHeaderId = Guid.Parse("14965490-d4ab-4374-a8f9-b2a3740c3e84");
        const string httpClientName = UofSdkBootstrap.HttpClientNameForNormal;

        SetupLoggerFactory(logLevel);
        SetupHttpClientWithResponseOk(httpClientName);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcher>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        _ = await logHttpFetcher.GetDataAsync(new Uri("http://localhost/"));

        _requestHeaderInspectorMock.Verify(d => d.VerifyRequestHeader(requestHeaderId.ToString()), Times.Once);
        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Critical)]
    public void HttpNormalClientDecoratesEachRequestWithTraceIdWhenCalledGetData(LogLevel logLevel)
    {
        var requestHeaderId = Guid.Parse("14965490-d4ab-4374-a8f9-b2a3740c3e84");
        const string httpClientName = UofSdkBootstrap.HttpClientNameForNormal;

        SetupLoggerFactory(logLevel);
        SetupHttpClientWithResponseOk(httpClientName);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcher>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        _ = logHttpFetcher.GetData(new Uri("http://localhost/"));

        _requestHeaderInspectorMock.Verify(d => d.VerifyRequestHeader(requestHeaderId.ToString()), Times.Once);
        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Critical)]
    public async Task HttpRecoveryClientDecoratesEachRequestWithTraceIdWhenCalledGetDataAsync(LogLevel logLevel)
    {
        var requestHeaderId = Guid.Parse("24965490-d4ab-4374-a8f9-b2a3740c3e84");
        const string httpClientName = UofSdkBootstrap.HttpClientNameForRecovery;

        SetupLoggerFactory(logLevel);
        SetupHttpClientWithResponseOk(httpClientName);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcherRecovery>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        _ = await logHttpFetcher.GetDataAsync(new Uri("http://localhost/"));

        _requestHeaderInspectorMock.Verify(d => d.VerifyRequestHeader(requestHeaderId.ToString()), Times.Once);
        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Critical)]
    public void HttpRecoveryClientDecoratesEachRequestWithTraceIdWhenCalledGetData(LogLevel logLevel)
    {
        var requestHeaderId = Guid.Parse("24965490-d4ab-4374-a8f9-b2a3740c3e84");
        const string httpClientName = UofSdkBootstrap.HttpClientNameForRecovery;

        SetupLoggerFactory(logLevel);
        SetupHttpClientWithResponseOk(httpClientName);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcherRecovery>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        _ = logHttpFetcher.GetData(new Uri("http://localhost/"));

        _requestHeaderInspectorMock.Verify(d => d.VerifyRequestHeader(requestHeaderId.ToString()), Times.Once);
        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));
    }

    [Fact]
    public async Task HttpFastFailingRequestHasTraceIdWhenExceptionIsThrownInGetDataAsync()
    {
        var requestHeaderId = Guid.Parse("34965490-d4ab-4374-a8f9-b2a3740c3e81");
        const string httpClientName = UofSdkBootstrap.HttpClientNameForFastFailing;

        SetupLoggerFactory(LogLevel.Trace);
        SetupHttpClientWithResponseBadRequest(httpClientName);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcherFastFailing>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        var exception = await Should.ThrowAsync<CommunicationException>(
                                                                        async () => await logHttpFetcher.GetDataAsync(new Uri("http://localhost/"))
                                                                       );

        _requestHeaderInspectorMock.Verify(d => d.VerifyRequestHeader(requestHeaderId.ToString()), Times.Once);
        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));
        exception.ShouldNotBeNull();
    }

    [Fact]
    public async Task HttpFastFailingRequestHasTraceIdWhenNotFoundIsReturnedByGetDataAsync()
    {
        var requestHeaderId = Guid.Parse("34965490-d4ab-4374-a8f9-b2a3740c3e81");
        const string httpClientName = UofSdkBootstrap.HttpClientNameForFastFailing;

        SetupLoggerFactory(LogLevel.Trace);
        SetupHttpClientWithResponseNotFoundRequest(httpClientName);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcherFastFailing>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        var exception = await Should.ThrowAsync<CommunicationException>(
                                                                        async () => await logHttpFetcher.GetDataAsync(new Uri("http://localhost/"))
                                                                       );

        _requestHeaderInspectorMock.Verify(d => d.VerifyRequestHeader(requestHeaderId.ToString()), Times.Once);
        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));
        exception.ShouldNotBeNull();
    }

    [Fact]
    public async Task HttpFastFailingRequestHasTraceIdWhenRequestTriggeredTimoutInGetDataAsync()
    {
        var requestHeaderId = Guid.Parse("34965490-d4ab-4374-a8f9-b2a3740c3e81");
        const string httpClientName = UofSdkBootstrap.HttpClientNameForFastFailing;

        SetupLoggerFactory(LogLevel.Trace);
        SetupTimeoutHttpClientDependencyInjection(httpClientName);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcherFastFailing>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        var exception = await Should.ThrowAsync<CommunicationException>(
                                                                        async () => await logHttpFetcher.GetDataAsync(new Uri("http://localhost/"))
                                                                       );

        _requestHeaderInspectorMock.Verify(d => d.VerifyRequestHeader(requestHeaderId.ToString()), Times.Once);
        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));
        exception.ShouldNotBeNull();
    }

    [Fact]
    public async Task HttpRequestHasTraceIdWhenRequestTriggeredTimoutInGetDataAsync()
    {
        var requestHeaderId = Guid.Parse("34965490-d4ab-4374-a8f9-b2a3740c3843");
        const string httpClientName = UofSdkBootstrap.HttpClientNameForNormal;

        SetupLoggerFactory(LogLevel.Trace);
        SetupTimeoutHttpClientDependencyInjection(httpClientName);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcher>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        var exception = await Should.ThrowAsync<CommunicationException>(
                                                                        async () => await logHttpFetcher.GetDataAsync(new Uri("http://localhost/"))
                                                                       );

        _requestHeaderInspectorMock.Verify(d => d.VerifyRequestHeader(requestHeaderId.ToString()), Times.Once);
        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));
        exception.ShouldNotBeNull();
    }

    [Fact]
    public async Task HttpRecoveryRequestHasTraceIdWhenRequestTriggeredTimoutInPostDataAsync()
    {
        var requestHeaderId = Guid.Parse("34965490-d4ab-4374-a8f9-b2a3740c3843");
        const string httpClientName = UofSdkBootstrap.HttpClientNameForRecovery;

        SetupLoggerFactory(LogLevel.Trace);
        SetupTimeoutHttpClientDependencyInjection(httpClientName);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcherRecovery>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        var exception = await Should.ThrowAsync<CommunicationException>(
                                                                        async () => await logHttpFetcher.PostDataAsync(new Uri("http://localhost/"))
                                                                       );

        _requestHeaderInspectorMock.Verify(d => d.VerifyRequestHeader(requestHeaderId.ToString()), Times.Once);
        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));
        exception.ShouldNotBeNull();
    }

    [Fact]
    public async Task HttpRequestHasTraceIdWhenNotFoundIsReturnedByGetDataAsync()
    {
        var requestHeaderId = Guid.Parse("34965490-d4ab-4374-a8f9-b2a3740c3e81");
        const string httpClientName = UofSdkBootstrap.HttpClientNameForNormal;

        SetupLoggerFactory(LogLevel.Trace);
        SetupHttpClientWithResponseNotFoundRequest(httpClientName);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcher>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        var exception = await Should.ThrowAsync<CommunicationException>(
                                                                        async () => await logHttpFetcher.GetDataAsync(new Uri("http://localhost/"))
                                                                       );

        _requestHeaderInspectorMock.Verify(d => d.VerifyRequestHeader(requestHeaderId.ToString()), Times.Once);
        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));
        exception.ShouldNotBeNull();
    }

    [Fact]
    public async Task HttpRecoveryRequestHasTraceIdWhenNotFoundIsReturnedByGetDataAsync()
    {
        var requestHeaderId = Guid.Parse("34965490-d4ab-4374-a8f9-b2a3740c3e81");
        const string httpClientName = UofSdkBootstrap.HttpClientNameForRecovery;

        SetupLoggerFactory(LogLevel.Trace);
        SetupHttpClientWithResponseNotFoundRequest(httpClientName);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcherRecovery>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        var exception = await Should.ThrowAsync<CommunicationException>(
                                                                        async () => await logHttpFetcher.GetDataAsync(new Uri("http://localhost/"))
                                                                       );

        _requestHeaderInspectorMock.Verify(d => d.VerifyRequestHeader(requestHeaderId.ToString()), Times.Once);
        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));
        exception.ShouldNotBeNull();
    }

    [Fact]
    public void HttpFastFailingRequestHasTraceIdWhenExceptionIsThrownInGetData()
    {
        var requestHeaderId = Guid.Parse("34965490-d4ab-4374-a8f9-b2a3740c3e82");
        const string httpClientName = UofSdkBootstrap.HttpClientNameForFastFailing;

        SetupLoggerFactory(LogLevel.Trace);
        SetupHttpClientWithResponseBadRequest(httpClientName);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcherFastFailing>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        var exception = Should.Throw<CommunicationException>(() => logHttpFetcher.GetData(new Uri("http://localhost/")));

        _requestHeaderInspectorMock.Verify(d => d.VerifyRequestHeader(requestHeaderId.ToString()), Times.Once);
        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));
        exception.ShouldNotBeNull();
    }

    [Fact]
    public async Task HttpNormalRequestHasTraceIdWhenExceptionIsThrownInGetDataAsync()
    {
        var requestHeaderId = Guid.Parse("34965490-d4ab-4374-a8f9-b2a3740c3e83");
        const string httpClientName = UofSdkBootstrap.HttpClientNameForNormal;

        SetupLoggerFactory(LogLevel.Trace);
        SetupHttpClientWithResponseBadRequest(httpClientName);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcher>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        var exception = await Should.ThrowAsync<CommunicationException>(
                                                                        async () => await logHttpFetcher.GetDataAsync(new Uri("http://localhost/"))
                                                                       );

        _requestHeaderInspectorMock.Verify(d => d.VerifyRequestHeader(requestHeaderId.ToString()), Times.Once);
        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));
        exception.ShouldNotBeNull();
    }

    [Fact]
    public void HttpNormalRequestHasTraceIdWhenExceptionIsThrownInGetData()
    {
        var requestHeaderId = Guid.Parse("34965490-d4ab-4374-a8f9-b2a3740c3e84");
        const string httpClientName = UofSdkBootstrap.HttpClientNameForNormal;

        SetupLoggerFactory(LogLevel.Trace);
        SetupHttpClientWithResponseBadRequest(httpClientName);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcher>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        var exception = Should.Throw<CommunicationException>(() => logHttpFetcher.GetData(new Uri("http://localhost/")));

        _requestHeaderInspectorMock.Verify(d => d.VerifyRequestHeader(requestHeaderId.ToString()), Times.Once);
        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));
        exception.ShouldNotBeNull();
    }

    [Fact]
    public async Task HttpRecoveryRequestHasTraceIdWhenExceptionIsThrownInGetDataAsync()
    {
        var requestHeaderId = Guid.Parse("34965490-d4ab-4374-a8f9-b2a3740c3e85");
        const string httpClientName = UofSdkBootstrap.HttpClientNameForRecovery;

        SetupLoggerFactory(LogLevel.Trace);
        SetupHttpClientWithResponseBadRequest(httpClientName);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcherRecovery>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        var exception = await Should.ThrowAsync<CommunicationException>(
                                                                        async () => await logHttpFetcher.GetDataAsync(new Uri("http://localhost/"))
                                                                       );

        _requestHeaderInspectorMock.Verify(d => d.VerifyRequestHeader(requestHeaderId.ToString()), Times.Once);
        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));
        exception.ShouldNotBeNull();
    }

    [Fact]
    public void HttpRecoveryRequestHasTraceIdWhenExceptionIsThrownInGetData()
    {
        var requestHeaderId = Guid.Parse("34965490-d4ab-4374-a8f9-b2a3740c3e86");
        const string httpClientName = UofSdkBootstrap.HttpClientNameForRecovery;

        SetupLoggerFactory(LogLevel.Trace);
        SetupHttpClientWithResponseBadRequest(httpClientName);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcherRecovery>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        var exception = Should.Throw<CommunicationException>(() => logHttpFetcher.GetData(new Uri("http://localhost/")));

        _requestHeaderInspectorMock.Verify(d => d.VerifyRequestHeader(requestHeaderId.ToString()), Times.Once);
        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));
        exception.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Critical)]
    public async Task HttpFastFailingClientDecoratesEachRequestWithTraceIdWhenCallPostAsync(LogLevel logLevel)
    {
        var requestHeaderId = Guid.Parse("55965490-d4ab-4374-a8f9-b2a3740c3e84");
        const string httpClientName = UofSdkBootstrap.HttpClientNameForFastFailing;

        SetupLoggerFactory(logLevel);
        SetupHttpClientWithResponseOk(httpClientName);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcherFastFailing>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        var response = await logHttpFetcher.PostDataAsync(new Uri("http://localhost/"), new StringContent(string.Empty));

        _requestHeaderInspectorMock.Verify(d => d.VerifyRequestHeader(requestHeaderId.ToString()), Times.Once);
        response.RequestMessage.ShouldNotBeNull();
        response.RequestMessage.Headers.ShouldContain(h => string.Equals(h.Key, HttpApiConstants.TraceIdHeaderName, StringComparison.OrdinalIgnoreCase));
        response.RequestMessage.Headers.GetValues(HttpApiConstants.TraceIdHeaderName).First().ShouldBe(requestHeaderId.ToString());
        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Critical)]
    public async Task HttpNormalClientDecoratesEachRequestWithTraceIdWhenCallPostAsync(LogLevel logLevel)
    {
        var requestHeaderId = Guid.Parse("80965490-d4ab-4374-a8f9-b2a3740c3e84");
        const string httpClientName = UofSdkBootstrap.HttpClientNameForNormal;

        SetupLoggerFactory(logLevel);
        SetupHttpClientWithResponseOk(httpClientName);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcher>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        var response = await logHttpFetcher.PostDataAsync(new Uri("http://localhost/"), new StringContent(string.Empty));

        _requestHeaderInspectorMock.Verify(d => d.VerifyRequestHeader(requestHeaderId.ToString()), Times.Once);
        response.RequestMessage.ShouldNotBeNull();
        response.RequestMessage.Headers.ShouldContain(h => string.Equals(h.Key, HttpApiConstants.TraceIdHeaderName, StringComparison.OrdinalIgnoreCase));
        response.RequestMessage.Headers.GetValues(HttpApiConstants.TraceIdHeaderName).First().ShouldBe(requestHeaderId.ToString());
        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Critical)]
    public async Task HttpRecoveryClientDecoratesEachRequestWithTraceIdWhenCallPostAsync(LogLevel logLevel)
    {
        var requestHeaderId = Guid.Parse("75965490-d4ab-4374-a8f9-b2a3740c3e84");
        const string httpClientName = UofSdkBootstrap.HttpClientNameForRecovery;

        SetupLoggerFactory(logLevel);
        SetupHttpClientWithResponseOk(httpClientName);
        SetupRequestDecoratorWithHeader(TraceIdHeaderName, requestHeaderId.ToString());

        var serviceProvider = _serviceCollection.BuildServiceProvider(true);
        var logHttpFetcher = serviceProvider.GetRequiredService<ILogHttpDataFetcherRecovery>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var restLogger = (XUnitLogger)loggerFactory.CreateLogger(It.IsAny<string>());

        var response = await logHttpFetcher.PostDataAsync(new Uri("http://localhost/"), new StringContent(string.Empty));

        _requestHeaderInspectorMock.Verify(d => d.VerifyRequestHeader(requestHeaderId.ToString()), Times.Once);
        response.RequestMessage.ShouldNotBeNull();
        response.RequestMessage.Headers.ShouldContain(h => string.Equals(h.Key, HttpApiConstants.TraceIdHeaderName, StringComparison.OrdinalIgnoreCase));
        response.RequestMessage.Headers.GetValues(HttpApiConstants.TraceIdHeaderName).First().ShouldBe(requestHeaderId.ToString());
        restLogger.Messages.ShouldNotBeEmpty();
        restLogger.Messages.ShouldContain(m => m.Contains(requestHeaderId.ToString()));
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

    private void SetupHttpClientWithResponseBadRequest(string httpClientName)
    {
        SetupHttpClientDependencyInjection(httpClientName, HttpStatusCode.BadRequest);
    }

    private void SetupHttpClientWithResponseNotFoundRequest(string httpClientName)
    {
        SetupHttpClientDependencyInjection(httpClientName, HttpStatusCode.NotFound);
    }

    private void SetupHttpClientWithResponseOk(string httpClientName)
    {
        SetupHttpClientDependencyInjection(httpClientName, HttpStatusCode.OK);
    }

    private void SetupHttpClientDependencyInjection(string httpClientName, HttpStatusCode httpStatusCode)
    {
        _serviceCollection.AddHttpClient(httpClientName)
                          .ConfigureHttpClient(configureClient =>
                                               {
                                                   configureClient.Timeout = TimeSpan.FromMinutes(1);
                                                   configureClient.DefaultRequestHeaders.Add(UofSdkBootstrap.HttpClientDefaultRequestHeaderForAccessToken, "token");
                                                   configureClient.DefaultRequestHeaders.Add(UofSdkBootstrap.HttpClientDefaultRequestHeaderForUserAgent, "User Agent");
                                               })
                          .ConfigurePrimaryHttpMessageHandler(() =>
                                                                  new HttpRequestDecoratorHandler(_requestDecoratorMock.Object, new StubHttpClientHandler(_requestHeaderInspectorMock.Object, httpStatusCode)
                                                                  {
                                                                      MaxConnectionsPerServer = 100,
                                                                      AllowAutoRedirect = true
                                                                  }));
    }

    private void SetupTimeoutHttpClientDependencyInjection(string httpClientName)
    {
        _serviceCollection.AddHttpClient(httpClientName)
                          .ConfigureHttpClient(configureClient =>
                                               {
                                                   configureClient.Timeout = TimeSpan.FromMinutes(1);
                                                   configureClient.DefaultRequestHeaders.Add(UofSdkBootstrap.HttpClientDefaultRequestHeaderForAccessToken, "token");
                                                   configureClient.DefaultRequestHeaders.Add(UofSdkBootstrap.HttpClientDefaultRequestHeaderForUserAgent, "User Agent");
                                               })
                          .ConfigurePrimaryHttpMessageHandler(() =>
                                                                  new HttpRequestDecoratorHandler(_requestDecoratorMock.Object, new StubHttpClientTimeoutHandler(_requestHeaderInspectorMock.Object)
                                                                  {
                                                                      MaxConnectionsPerServer = 100,
                                                                      AllowAutoRedirect = true
                                                                  }));
    }

    private static void ConfigureApiConfig(Mock<IUofConfiguration> uofConfigMock)
    {
        uofConfigMock.Setup(config => config.Api)
                     .Returns(new UofApiConfiguration
                     {
                         MaxConnectionsPerServer = 100,
                         Host = "localhost",
                         UseSsl = false,
                         HttpClientTimeout = TimeSpan.FromMinutes(1),
                         HttpClientRecoveryTimeout = TimeSpan.FromMinutes(1),
                         HttpClientFastFailingTimeout = TimeSpan.FromMinutes(1),
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
                         IgnoreBetPalTimelineSportEventStatusCacheTimeout = TimeSpan.FromMinutes(1),
                     });
    }

    private class StubHttpClientHandler : HttpClientHandler
    {
        private readonly IRequestHeaderInspector _headerInspector;
        private readonly HttpStatusCode _statusCode;
        private readonly string _responseContent;

        public StubHttpClientHandler(IRequestHeaderInspector headerInspector,
            HttpStatusCode statusCode = HttpStatusCode.OK,
            string responseContent = "")
        {
            _headerInspector = headerInspector;
            _statusCode = statusCode;
            _responseContent = responseContent;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            foreach (var header in request.Headers)
            {
                foreach (var headerValue in header.Value)
                {
                    _headerInspector.VerifyRequestHeader(headerValue);
                }
            }

            return await Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                RequestMessage = request,
                Content = new StringContent(_responseContent),
            });
        }
    }

    private class StubHttpClientTimeoutHandler : HttpClientHandler
    {
        private readonly IRequestHeaderInspector _headerInspector;

        public StubHttpClientTimeoutHandler(IRequestHeaderInspector headerInspector)
        {
            _headerInspector = headerInspector;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            foreach (var header in request.Headers)
            {
                foreach (var headerValue in header.Value)
                {
                    _headerInspector.VerifyRequestHeader(headerValue);
                }
            }

            throw new TaskCanceledException("Timeout");
        }
    }
}
