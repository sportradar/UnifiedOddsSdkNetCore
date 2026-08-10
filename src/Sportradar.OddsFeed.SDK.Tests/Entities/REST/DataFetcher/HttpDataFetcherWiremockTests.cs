// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.DataFetcher;

public class HttpDataFetcherWiremockTests : IAsyncLifetime
{
    private const string AccessToken = "aaa";
    private const string UserAgent = "UofSdk-Net";
    private WireMockServer _wireMockServer;
    private Uri _serverUri;

    public Task InitializeAsync()
    {
        _wireMockServer = WireMockServer.Start();
        _serverUri = new Uri(_wireMockServer.Url!);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _wireMockServer?.Stop();
        _wireMockServer?.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetDataAsyncWithHeadersSendsProvidedHeaders()
    {
        const string endpoint = "/header-check";
        const string headerKey = "x-test-header";
        const string headerValue = "test-value";
        const string responseBody = "<response>ok</response>";

        _wireMockServer
            .Given(Request.Create().WithPath(endpoint).UsingGet().WithHeader(headerKey, headerValue))
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(responseBody));

        var fetcher = CreateFetcherWithDefaultHeadersUsingHttpClient("wiremock-headers");

        await using var stream = await fetcher.GetDataAsync(new Uri(_serverUri, endpoint), null, new Dictionary<string, string> { { headerKey, headerValue } });
        using var streamReader = new StreamReader(stream);
        var content = await streamReader.ReadToEndAsync();

        content.ShouldBe(responseBody);
    }

    [Fact]
    public async Task GetDataAsyncWithNullHeadersSendsNoCustomHeaders()
    {
        const string endpoint = "/header-check-null";
        const string headerKey = "x-test-header";
        const string expectedBody = "<response>ok</response>";
        const string unexpectedBody = "<response>unexpected-header</response>";

        _wireMockServer
            .Given(Request.Create().WithPath(endpoint).UsingGet().WithHeader(headerKey, new RegexMatcher(".+")))
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(unexpectedBody));

        _wireMockServer
            .Given(Request.Create().WithPath(endpoint).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(expectedBody));

        var fetcher = CreateFetcherWithDefaultHeadersUsingHttpClient("wiremock-null-headers");

        await using var stream = await fetcher.GetDataAsync(new Uri(_serverUri, endpoint), null, null);
        using var streamReader = new StreamReader(stream);
        var content = await streamReader.ReadToEndAsync();

        content.ShouldBe(expectedBody);
    }

    [Fact]
    public async Task GetDataAsyncWithQueryParametersSendsProvidedQueryParameters()
    {
        const string endpoint = "/query-check";
        const string responseBody = "<response>query-ok</response>";

        _wireMockServer
            .Given(Request.Create()
                          .WithPath(endpoint)
                          .WithParam("marketId", "10")
                          .WithParam("outcomeId", "9")
                          .UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(responseBody));

        var fetcher = CreateFetcherWithDefaultHeadersUsingHttpClient("wiremock-query");
        var queryParameters = new Dictionary<string, string>
        {
            { "marketId", "10" },
            { "outcomeId", "9" }
        };

        await using var stream = await fetcher.GetDataAsync(new Uri(_serverUri, endpoint), queryParameters, null);
        using var streamReader = new StreamReader(stream);
        var content = await streamReader.ReadToEndAsync();

        content.ShouldBe(responseBody);
    }

    [Fact]
    public async Task GetDataAsyncWithNullQueryParametersBehavesAsNoQueryParameters()
    {
        const string endpoint = "/query-null-check";
        const string headerKey = "x-test-header";
        const string headerValue = "test-value";
        const string expectedBody = "<response>no-query</response>";
        const string unexpectedBody = "<response>query-present</response>";

        _wireMockServer
            .Given(Request.Create()
                          .WithPath(endpoint)
                          .WithParam(headerKey, new RegexMatcher(".+"))
                          .UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(unexpectedBody));

        _wireMockServer
            .Given(Request.Create()
                          .WithPath(endpoint)
                          .WithHeader(headerKey, headerValue)
                          .UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(expectedBody));

        var fetcher = CreateFetcherWithDefaultHeadersUsingHttpClient("wiremock-null-query");

        await using var stream = await fetcher.GetDataAsync(new Uri(_serverUri, endpoint), null, new Dictionary<string, string> { { headerKey, headerValue } });
        using var streamReader = new StreamReader(stream);
        var content = await streamReader.ReadToEndAsync();

        content.ShouldBe(expectedBody);
    }

    [Fact]
    public async Task GetDataAsyncWithQueryParametersWhenValueIsNullEncodesAsEmptyValue()
    {
        const string endpoint = "/query-null-value";
        const string expectedBody = "<response>ok</response>";
        const string unexpectedBody = "<response>unexpected</response>";

        _wireMockServer
            .Given(Request.Create().WithPath(endpoint).WithParam("key", string.Empty).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(expectedBody));

        _wireMockServer
            .Given(Request.Create().WithPath(endpoint).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(unexpectedBody));

        var fetcher = CreateFetcherWithDefaultHeadersUsingHttpClient("wiremock-null-value");
        var queryParameters = new Dictionary<string, string> { { "key", null } };

        await using var stream = await fetcher.GetDataAsync(new Uri(_serverUri, endpoint), queryParameters, null);
        using var streamReader = new StreamReader(stream);
        var content = await streamReader.ReadToEndAsync();
        content.ShouldBe(expectedBody);
    }

    [Fact]
    public async Task GetDataAsyncWithQueryParametersUrlEncodesKeyAndValue()
    {
        const string endpoint = "/query-encoded";
        const string expectedBody = "<response>encoded-ok</response>";
        const string unexpectedBody = "<response>unexpected</response>";
        const string queryKey = "marketId";
        const string queryValue = "special=value&more";

        _wireMockServer
            .Given(Request.Create().WithPath(endpoint).WithParam(queryKey, queryValue).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(expectedBody));

        _wireMockServer
            .Given(Request.Create().WithPath(endpoint).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(unexpectedBody));

        var fetcher = CreateFetcherWithDefaultHeadersUsingHttpClient("wiremock-encoded-query");
        var queryParameters = new Dictionary<string, string> { { queryKey, queryValue } };

        await using var stream = await fetcher.GetDataAsync(new Uri(_serverUri, endpoint), queryParameters, null);
        using var streamReader = new StreamReader(stream);
        var content = await streamReader.ReadToEndAsync();
        content.ShouldBe(expectedBody);
    }

    [Fact]
    public async Task GetDataAsyncWithHeadersWhenTaskCanceledThrowsCommunicationExceptionWithRequestTimeout()
    {
        const string endpoint = "/delayed";

        _wireMockServer
            .Given(Request.Create().WithPath(endpoint).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithDelay(200).WithBody("<response>ok</response>"));

        var fetcher = CreateFetcherWithDefaultHeadersUsingHttpClient("wiremock-timeout", TimeSpan.FromMilliseconds(50));

        var exception = await Should.ThrowAsync<CommunicationException>(() =>
            fetcher.GetDataAsync(new Uri(_serverUri, endpoint), null, new Dictionary<string, string> { { "x-test", "1" } }));

        exception.ResponseCode.ShouldBe(HttpStatusCode.RequestTimeout);
        exception.InnerException.ShouldBeOfType<TaskCanceledException>();
    }

    [Fact]
    public async Task GetDataAsyncWithHeadersWhenUnexpectedExceptionThrowsCommunicationExceptionWithOkStatus()
    {
        var stoppedServerUri = _serverUri;
        _wireMockServer.Stop();

        var fetcher = CreateFetcherWithDefaultHeadersUsingHttpClient("wiremock-server-down");

        var exception = await Should.ThrowAsync<CommunicationException>(() =>
            fetcher.GetDataAsync(new Uri(stoppedServerUri, "/server-down"), null, new Dictionary<string, string> { { "x-test", "1" } }));

        exception.ResponseCode.ShouldBe(HttpStatusCode.OK);
        exception.InnerException.ShouldBeOfType<HttpRequestException>();
    }

    [Fact]
    public async Task GetDataAsyncWithHeadersWhenUriContainsQueryThrowsArgumentException()
    {
        var fetcher = CreateFetcherWithDefaultHeadersUsingHttpClient("wiremock-query-in-uri");
        var uriWithQuery = new Uri(_serverUri, "/with-query?marketId=10");

        var exception = await Should.ThrowAsync<ArgumentException>(() =>
            fetcher.GetDataAsync(uriWithQuery, null, new Dictionary<string, string> { { "x-test", "1" } }));

        exception.ParamName.ShouldBe("uri");
    }

    private static HttpDataFetcher CreateFetcherWithDefaultHeadersUsingHttpClient(string httpClientName, TimeSpan? timeout = null)
    {
        var services = new ServiceCollection();
        services.AddHttpClient(httpClientName)
            .ConfigureHttpClient(configureClient =>
                                 {
                                     configureClient.Timeout = timeout ?? TimeSpan.FromSeconds(5);
                                     configureClient.DefaultRequestHeaders.Add(UofSdkBootstrap.HttpClientDefaultRequestHeaderForAccessToken, AccessToken);
                                     configureClient.DefaultRequestHeaders.Add(UofSdkBootstrap.HttpClientDefaultRequestHeaderForUserAgent, UserAgent);
                                 });

        var serviceProvider = services.BuildServiceProvider();
        var sdkHttpClient = new SdkHttpClient(serviceProvider.GetRequiredService<IHttpClientFactory>(), httpClientName);

        return new HttpDataFetcher(sdkHttpClient, new Deserializer<response>());
    }
}
