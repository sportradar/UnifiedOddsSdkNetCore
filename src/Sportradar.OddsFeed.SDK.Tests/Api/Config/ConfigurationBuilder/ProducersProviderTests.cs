// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API.Config.ConfigurationBuilder;

public class ProducersProviderTests : ConfigurationBuilderWithSectionSetup
{
    private IUofConfiguration _configCustom;
    private StubProducerProvider _apiProducerProvider;
    private readonly IProducersProvider _producersProvider;
    private IProducersProvider _producersProviderCustom;

    public ProducersProviderTests()
    {
        var configIntegration = new TokenSetter(new TestSectionProvider(TestSection.GetDefaultSection()), new TestBookmakerDetailsProvider(), new TestProducersProvider())
                               .SetAccessToken("some-access-token")
                               .SelectEnvironment(SdkEnvironment.Integration)
                               .SetDesiredLanguages(TestData.Cultures1)
                               .Build();

        _configCustom = new TokenSetter(new TestSectionProvider(TestSection.GetCustomSection()), new TestBookmakerDetailsProvider(), new TestProducersProvider())
                       .SetAccessToken("some-access-token")
                       .SelectCustom()
                       .SetDesiredLanguages(TestData.Cultures1)
                       .SetExceptionHandlingStrategy(ExceptionHandlingStrategy.Throw)
                       .SetApiHost(CustomApiHost)
                       .UseApiSsl(true)
                       .Build();

        _apiProducerProvider = new StubProducerProvider();
        _producersProvider = new ProducersProvider(_apiProducerProvider, configIntegration);
        _producersProviderCustom = new ProducersProvider(_apiProducerProvider, _configCustom);
    }

    [Fact]
    public void BaseApiProducerProviderReturnsProducersWithIntegrationApiUrl()
    {
        var producers = _apiProducerProvider.Producers.producer.ToList();

        Assert.True(producers.TrueForAll(a => !a.api_url.Contains(CustomApiHost)));
    }

    [Fact]
    public void NormalConfigurationWhenIsBuildThenProducerApiUrlIsNotRewritten()
    {
        var resultProducer = _producersProvider.GetProducers();

        Assert.True(resultProducer.All(a => !((Producer)a).ApiUrl.Contains(CustomApiHost)));
    }

    [Fact]
    public void CustomConfigurationWhenProducersHaveStgApiUrlThenProducerApiUrlIsRewritten()
    {
        var resultProducer = _producersProviderCustom.GetProducers();

        Assert.True(resultProducer.All(a => ((Producer)a).ApiUrl.Contains(CustomApiHost)));
        Assert.True(resultProducer.All(a => ((Producer)a).ApiUrl.StartsWith("https://", StringComparison.InvariantCulture)));
    }

    [Fact]
    public void CustomConfigurationWhenProducersHaveProdApiUrlThenProducerApiUrlIsRewritten()
    {
        var newProducers = _apiProducerProvider.Producers;
        foreach (var t in newProducers.producer)
        {
            t.api_url = t.api_url.Replace("stgapi.", "api.");
        }
        Assert.True(newProducers.producer.All(a => a.api_url.StartsWith("https://api.betradar.com/v1/", StringComparison.InvariantCulture)));
        _apiProducerProvider = new StubProducerProvider(newProducers);
        _producersProviderCustom = new ProducersProvider(_apiProducerProvider, _configCustom);

        var resultProducer = _producersProviderCustom.GetProducers();

        Assert.True(resultProducer.All(a => ((Producer)a).ApiUrl.Contains(CustomApiHost)));
        Assert.True(resultProducer.All(a => ((Producer)a).ApiUrl.StartsWith("https://", StringComparison.InvariantCulture)));
    }

    [Fact]
    public void CustomConfigurationWhenSetNoApiSslThenProducerApiUrlUsesHttp()
    {
        _configCustom = new TokenSetter(new TestSectionProvider(TestSection.GetCustomSection()), new TestBookmakerDetailsProvider(), new TestProducersProvider())
                       .SetAccessToken("some-access-token")
                       .SelectCustom()
                       .SetDesiredLanguages(TestData.Cultures1)
                       .SetExceptionHandlingStrategy(ExceptionHandlingStrategy.Throw)
                       .SetApiHost(CustomApiHost)
                       .UseApiSsl(false)
                       .Build();
        _producersProviderCustom = new ProducersProvider(_apiProducerProvider, _configCustom);

        var resultProducer = _producersProviderCustom.GetProducers();

        Assert.True(resultProducer.All(a => ((Producer)a).ApiUrl.Contains(CustomApiHost)));
        Assert.True(resultProducer.All(a => ((Producer)a).ApiUrl.StartsWith("http://", StringComparison.InvariantCulture)));
    }

    [Fact]
    public void CustomConfigurationWhenOnlySetNoApiSslThenProducerApiUrlUsesHttp()
    {
        _configCustom = new TokenSetter(new TestSectionProvider(TestSection.GetCustomSection()), new TestBookmakerDetailsProvider(), new TestProducersProvider())
                       .SetAccessToken("some-access-token")
                       .SelectCustom()
                       .SetDesiredLanguages(TestData.Cultures1)
                       .SetExceptionHandlingStrategy(ExceptionHandlingStrategy.Throw)
                       .UseApiSsl(false)
                       .Build();
        _producersProviderCustom = new ProducersProvider(_apiProducerProvider, _configCustom);

        var resultProducer = _producersProviderCustom.GetProducers();

        Assert.True(resultProducer.All(a => !((Producer)a).ApiUrl.Contains(CustomApiHost)));
        Assert.True(resultProducer.All(a => ((Producer)a).ApiUrl.StartsWith("http://", StringComparison.InvariantCulture)));
    }

    [Fact]
    public void CustomConfigurationWhenReceivedProducersHaveNonSslEndpointsAndSetToUseSslThenProducerApiUrlUsesHttps()
    {
        var newProducers = _apiProducerProvider.Producers;
        foreach (var prod in newProducers.producer)
        {
            prod.api_url = prod.api_url.Replace("https://", "http://");
        }
        Assert.True(newProducers.producer.All(a => a.api_url.StartsWith("http://", StringComparison.InvariantCulture)));
        _apiProducerProvider = new StubProducerProvider(newProducers);
        _producersProviderCustom = new ProducersProvider(_apiProducerProvider, _configCustom);

        var resultProducer = _producersProviderCustom.GetProducers();

        Assert.True(resultProducer.All(a => ((Producer)a).ApiUrl.Contains(CustomApiHost)));
        Assert.True(resultProducer.All(a => ((Producer)a).ApiUrl.StartsWith("https://", StringComparison.InvariantCulture)));
    }
}
