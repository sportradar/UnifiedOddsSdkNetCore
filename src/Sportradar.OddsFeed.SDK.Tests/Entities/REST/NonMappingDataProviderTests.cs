// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest;

public class NonMappingDataProviderTests
{
    private const string InputXml = "producers.xml";
    private readonly Mock<IDataFetcher> _dataFetcher;
    private readonly NonMappingDataProvider<producers> _producersProvider;
    private const string DefaultLanguageCode = "en";

    public NonMappingDataProviderTests()
    {
        var url = TestData.RestXmlPath;
        _dataFetcher = new Mock<IDataFetcher>();
        var deserializer = new Deserializer<producers>();

        _producersProvider = new NonMappingDataProvider<producers>(url + InputXml, _dataFetcher.Object, deserializer);
    }

    [Fact]
    public void ImplementingCorrectInterface()
    {
        _ = Assert.IsAssignableFrom<IDataProvider<producers>>(_producersProvider);
    }

    [Fact]
    public void ConstructorWhenNullUrlFormatThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new NonMappingDataProvider<producers>(null, _dataFetcher.Object, new Deserializer<producers>()));
    }

    [Fact]
    public void ConstructorWhenEmptyUrlFormatThenThrow()
    {
        _ = Assert.Throws<ArgumentException>(() => new NonMappingDataProvider<producers>(string.Empty, _dataFetcher.Object, new Deserializer<producers>()));
    }

    [Fact]
    public void ConstructorWhenNullDataFetcherThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new NonMappingDataProvider<producers>(InputXml, null, new Deserializer<producers>()));
    }

    [Fact]
    public void ConstructorWhenNullDeserializerThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new NonMappingDataProvider<producers>(InputXml, _dataFetcher.Object, null));
    }

    [Fact]
    public void ConstructorWheInitializedThenDataFetcherIsExposed()
    {
        Assert.Same(_dataFetcher.Object, _producersProvider.DataFetcher);
    }

    [Fact]
    public async Task GetDataAsyncWhenIdentifierMissingThenThrow()
    {
        SetupDataFetcherWithValidResponse();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _producersProvider.GetDataAsync(Array.Empty<string>()));
    }

    [Fact]
    public async Task GetDataAsyncWhenIdentifierNullThenThrow()
    {
        SetupDataFetcherWithValidResponse();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _producersProvider.GetDataAsync((string)null));
    }

    [Fact]
    public async Task GetDataAsyncWhenCorrectResponseThenDataIsReturned()
    {
        SetupDataFetcherWithValidResponse();

        var producers = await _producersProvider.GetDataAsync(new[] { DefaultLanguageCode });

        Assert.NotNull(producers);
        Assert.NotNull(producers.producer);
        Assert.Equal(4, producers.producer.Length);
        Assert.Equal(response_code.OK, producers.response_code);
        Assert.True(producers.producer.All(a => a.active && !string.IsNullOrEmpty(a.api_url) && !string.IsNullOrEmpty(a.description) && !string.IsNullOrEmpty(a.name)));
    }

    [Fact]
    public async Task GetDataAsyncWhenWrongResponseThenReturnsValidAttributeData()
    {
        SetupDataFetcherWithInvalidAttributeResponse();

        var producers = await _producersProvider.GetDataAsync(new[] { DefaultLanguageCode });

        Assert.NotNull(producers);
        Assert.NotNull(producers.producer);
        Assert.Equal(2, producers.producer.Length);
        Assert.Null(producers.producer[0].api_url);
    }

    [Fact]
    public async Task GetDataAsyncWhenWrongSubNodeThenThrow()
    {
        SetupDataFetcherWithInvalidSubProducerResponse();

        var producers = await _producersProvider.GetDataAsync(new[] { DefaultLanguageCode });

        Assert.NotNull(producers);
        Assert.NotNull(producers.producer);
        Assert.Single(producers.producer);
    }

    [Fact]
    public async Task GetDataAsyncWhenWrongResponseThenThrow()
    {
        SetupDataFetcherWithInvalidProducersResponse();

        await Assert.ThrowsAsync<DeserializationException>(async () => await _producersProvider.GetDataAsync(new[] { DefaultLanguageCode }));
    }

    [Fact]
    public async Task GetDataAsyncWithLanguageCodeWhenCorrectResponseThenDataIsReturned()
    {
        SetupDataFetcherWithValidResponse();

        var producers = await _producersProvider.GetDataAsync(DefaultLanguageCode);

        Assert.NotNull(producers);
        Assert.NotNull(producers.producer);
        Assert.Equal(4, producers.producer.Length);
        Assert.Equal(response_code.OK, producers.response_code);
        Assert.True(producers.producer.All(a => a.active && !string.IsNullOrEmpty(a.api_url) && !string.IsNullOrEmpty(a.description) && !string.IsNullOrEmpty(a.name)));
    }

    [Fact]
    public async Task GetDataAsyncWithLanguageCodeWhenWrongResponseThenReturnsValidAttributeData()
    {
        SetupDataFetcherWithInvalidAttributeResponse();

        var producers = await _producersProvider.GetDataAsync(DefaultLanguageCode);

        Assert.NotNull(producers);
        Assert.NotNull(producers.producer);
        Assert.Equal(2, producers.producer.Length);
        Assert.Null(producers.producer[0].api_url);
    }

    [Fact]
    public async Task GetDataAsyncWithLanguageCodeWhenWrongSubNodeThenThrow()
    {
        SetupDataFetcherWithInvalidSubProducerResponse();

        var producers = await _producersProvider.GetDataAsync(DefaultLanguageCode);

        Assert.NotNull(producers);
        Assert.NotNull(producers.producer);
        Assert.Single(producers.producer);
    }

    [Fact]
    public async Task GetDataAsyncWithLanguageCodeWhenWrongResponseThenThrow()
    {
        SetupDataFetcherWithInvalidProducersResponse();

        await Assert.ThrowsAsync<DeserializationException>(async () => await _producersProvider.GetDataAsync(DefaultLanguageCode));
    }

    [Fact]
    public void GetDataWhenIdentifierMissingThenThrow()
    {
        SetupDataFetcherWithValidResponse();

        Assert.Throws<ArgumentOutOfRangeException>(() => _producersProvider.GetData(Array.Empty<string>()));
    }

    [Fact]
    public void GetDataWhenIdentifierNullThenThrow()
    {
        SetupDataFetcherWithValidResponse();

        Assert.Throws<ArgumentOutOfRangeException>(() => _producersProvider.GetData((string)null));
    }

    [Fact]
    public void GetDataWhenCorrectResponseThenDataIsReturned()
    {
        SetupDataFetcherWithValidResponse();

        var producers = _producersProvider.GetData(new[] { DefaultLanguageCode });

        Assert.NotNull(producers);
        Assert.NotNull(producers.producer);
        Assert.Equal(4, producers.producer.Length);
        Assert.Equal(response_code.OK, producers.response_code);
        Assert.True(producers.producer.All(a => a.active && !string.IsNullOrEmpty(a.api_url) && !string.IsNullOrEmpty(a.description) && !string.IsNullOrEmpty(a.name)));
    }

    [Fact]
    public void GetDataWhenWrongResponseThenReturnsValidAttributeData()
    {
        SetupDataFetcherWithInvalidAttributeResponse();

        var producers = _producersProvider.GetData(new[] { DefaultLanguageCode });

        Assert.NotNull(producers);
        Assert.NotNull(producers.producer);
        Assert.Equal(2, producers.producer.Length);
        Assert.Null(producers.producer[0].api_url);
    }

    [Fact]
    public void GetDataWhenWrongSubNodeThenThrow()
    {
        SetupDataFetcherWithInvalidSubProducerResponse();

        var producers = _producersProvider.GetData(new[] { DefaultLanguageCode });

        Assert.NotNull(producers);
        Assert.NotNull(producers.producer);
        Assert.Single(producers.producer);
    }

    [Fact]
    public void GetDataWhenWrongResponseThenThrow()
    {
        SetupDataFetcherWithInvalidProducersResponse();

        Assert.Throws<DeserializationException>(() => _producersProvider.GetData(new[] { DefaultLanguageCode }));
    }

    [Fact]
    public void GetDataWithLanguageCodeWhenCorrectResponseThenDataIsReturned()
    {
        SetupDataFetcherWithValidResponse();

        var producers = _producersProvider.GetData(DefaultLanguageCode);

        Assert.NotNull(producers);
        Assert.NotNull(producers.producer);
        Assert.Equal(4, producers.producer.Length);
        Assert.Equal(response_code.OK, producers.response_code);
        Assert.True(producers.producer.All(a => a.active && !string.IsNullOrEmpty(a.api_url) && !string.IsNullOrEmpty(a.description) && !string.IsNullOrEmpty(a.name)));
    }

    [Fact]
    public void GetDataWithLanguageCodeWhenWrongResponseThenReturnsValidAttributeData()
    {
        SetupDataFetcherWithInvalidAttributeResponse();

        var producers = _producersProvider.GetData(DefaultLanguageCode);

        Assert.NotNull(producers);
        Assert.NotNull(producers.producer);
        Assert.Equal(2, producers.producer.Length);
        Assert.Null(producers.producer[0].api_url);
    }

    [Fact]
    public void GetDataWithLanguageCodeWhenWrongSubNodeThenThrow()
    {
        SetupDataFetcherWithInvalidSubProducerResponse();

        var producers = _producersProvider.GetData(DefaultLanguageCode);

        Assert.NotNull(producers);
        Assert.NotNull(producers.producer);
        Assert.Single(producers.producer);
    }

    [Fact]
    public void GetDataWithLanguageCodeWhenWrongResponseThenThrow()
    {
        SetupDataFetcherWithInvalidProducersResponse();

        Assert.Throws<DeserializationException>(() => _producersProvider.GetData(DefaultLanguageCode));
    }

    private void SetupDataFetcherWithValidResponse()
    {
        var validXmlResponse = "<?xml version='1.0' encoding='UTF-8' standalone='yes'?>"
                               + "<producers response_code='OK'>"
                               + "<producer active='true' api_url='https://api.betradar.com/v1/liveodds/' description='Live Odds' name='LO' id='1'/>"
                               + "<producer active='true' api_url='https://api.betradar.com/v1/pre/' description='Betradar Ctrl' name='Ctrl' id='3'/>"
                               + "<producer active='true' api_url='https://api.betradar.com/v1/betpal/' description='BetPal' name='BetPal' id='4'/>"
                               + "<producer active='true' api_url='https://api.betradar.com/v1/premium_cricket/' description='Premium Cricket' name='PremiumCricket' id='5'/>"
                               + "</producers>";
        _dataFetcher.Setup(s => s.GetDataAsync(It.IsAny<Uri>())).ReturnsAsync(FileHelper.GetStreamFromString(validXmlResponse));
        _dataFetcher.Setup(s => s.GetData(It.IsAny<Uri>())).Returns(FileHelper.GetStreamFromString(validXmlResponse));
    }

    private void SetupDataFetcherWithInvalidAttributeResponse()
    {
        var invalidXmlResponse = "<?xml version='1.0' encoding='UTF-8' standalone='yes'?>"
                                 + "<producers response_code='OK'>"
                                 + "<producer active='true' apiurl='https://api.betradar.com/v1/liveodds/' description='Live Odds' name='LO' id='1'/>"
                                 + "<producer active='true' api_url='https://api.betradar.com/v1/pre/' description='Betradar Ctrl' name='Ctrl' id='3'/>"
                                 + "</producers>";
        _dataFetcher.Setup(s => s.GetDataAsync(It.IsAny<Uri>())).ReturnsAsync(FileHelper.GetStreamFromString(invalidXmlResponse));
        _dataFetcher.Setup(s => s.GetData(It.IsAny<Uri>())).Returns(FileHelper.GetStreamFromString(invalidXmlResponse));
    }

    private void SetupDataFetcherWithInvalidSubProducerResponse()
    {
        var invalidXmlResponse = "<?xml version='1.0' encoding='UTF-8' standalone='yes'?>"
                                 + "<producers response_code='OK'>"
                                 + "<prod active='true' api_url='https://api.betradar.com/v1/liveodds/' description='Live Odds' name='LO' id='1'/>"
                                 + "<producer active='true' api_url='https://api.betradar.com/v1/pre/' description='Betradar Ctrl' name='Ctrl' id='3'/>"
                                 + "</producers>";
        _dataFetcher.Setup(s => s.GetDataAsync(It.IsAny<Uri>())).ReturnsAsync(FileHelper.GetStreamFromString(invalidXmlResponse));
        _dataFetcher.Setup(s => s.GetData(It.IsAny<Uri>())).Returns(FileHelper.GetStreamFromString(invalidXmlResponse));
    }

    private void SetupDataFetcherWithInvalidProducersResponse()
    {
        var invalidXmlResponse = "<?xml version='1.0' encoding='UTF-8' standalone='yes'?>"
                                 + "<prod response_code='OK'>"
                                 + "<producer active='true' api_url='https://api.betradar.com/v1/liveodds/' description='Live Odds' name='LO' id='1'/>"
                                 + "<producer active='true' api_url='https://api.betradar.com/v1/pre/' description='Betradar Ctrl' name='Ctrl' id='3'/>"
                                 + "</prod>";
        _dataFetcher.Setup(s => s.GetDataAsync(It.IsAny<Uri>())).ReturnsAsync(FileHelper.GetStreamFromString(invalidXmlResponse));
        _dataFetcher.Setup(s => s.GetData(It.IsAny<Uri>())).Returns(FileHelper.GetStreamFromString(invalidXmlResponse));
    }
}
