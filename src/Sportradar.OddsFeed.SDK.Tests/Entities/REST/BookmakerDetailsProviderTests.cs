// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Net;
using System.Threading.Tasks;
using Moq;
using Sportradar.OddsFeed.SDK.Api.EventArguments;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest;

public class BookmakerDetailsProviderTests
{
    private const string InputXml = "whoami.xml";
    private readonly Mock<IDataFetcher> _dataFetcher;
    private readonly BookmakerDetailsProvider _bookmakerDetailsProvider;
    private const string DefaultLanguageCode = "en";

    public BookmakerDetailsProviderTests()
    {
        var url = TestData.RestXmlPath;
        _dataFetcher = new Mock<IDataFetcher>();
        var deserializer = new Deserializer<bookmaker_details>();
        var mapperFactory = new BookmakerDetailsMapperFactory();

        _bookmakerDetailsProvider = new BookmakerDetailsProvider(url + InputXml, _dataFetcher.Object, deserializer, mapperFactory);
    }

    [Fact]
    public void ImplementingCorrectInterface()
    {
        _ = Assert.IsAssignableFrom<IDataProvider<BookmakerDetailsDto>>(_bookmakerDetailsProvider);
    }

    [Fact]
    public void ConstructorWhenNullUrlFormatThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new BookmakerDetailsProvider(null, _dataFetcher.Object, new Deserializer<bookmaker_details>(), new BookmakerDetailsMapperFactory()));
    }

    [Fact]
    public void ConstructorWhenEmptyUrlFormatThenThrow()
    {
        _ = Assert.Throws<ArgumentException>(() => new BookmakerDetailsProvider(string.Empty, _dataFetcher.Object, new Deserializer<bookmaker_details>(), new BookmakerDetailsMapperFactory()));
    }

    [Fact]
    public void ConstructorWhenNullDataFetcherThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new BookmakerDetailsProvider(InputXml, null, new Deserializer<bookmaker_details>(), new BookmakerDetailsMapperFactory()));
    }

    [Fact]
    public void ConstructorWhenNullDeserializerThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new BookmakerDetailsProvider(InputXml, _dataFetcher.Object, null, new BookmakerDetailsMapperFactory()));
    }

    [Fact]
    public void ConstructorWhenNullMapperFactoryThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new BookmakerDetailsProvider(InputXml, _dataFetcher.Object, new Deserializer<bookmaker_details>(), null));
    }

    [Fact]
    public void ConstructorWheInitializedThenDataFetcherIsExposed()
    {
        Assert.Same(_dataFetcher.Object, _bookmakerDetailsProvider.DataFetcher);
    }

    [Fact]
    public async Task GetDataAsyncWhenIdentifierEmptyThenReturnData()
    {
        SetupDataFetcherWithValidResponse();

        var dto = await _bookmakerDetailsProvider.GetDataAsync(Array.Empty<string>());

        Assert.NotNull(dto);
    }

    [Fact]
    public async Task GetDataAsyncWhenCorrectResponseThenDataIsReturned()
    {
        SetupDataFetcherWithValidResponse();

        var dto = await _bookmakerDetailsProvider.GetDataAsync([DefaultLanguageCode]);

        Assert.NotNull(dto);
        Assert.Equal(6908, dto.Id);
        Assert.Equal("/unifiedfeed/6908", dto.VirtualHost);
        Assert.Equal(HttpStatusCode.OK, dto.ResponseCode);
    }

    [Fact]
    public async Task GetDataAsyncWhenCorrectResponseThenDefaultServerDifferenceIsZero()
    {
        SetupDataFetcherWithValidResponse();

        var dto = await _bookmakerDetailsProvider.GetDataAsync([DefaultLanguageCode]);

        Assert.NotNull(dto);
        Assert.Equal(TimeSpan.Zero, dto.ServerTimeDifference);
    }

    [Fact]
    public async Task GetDataAsyncWhenWrongResponseThenReturnsValidAttributeData()
    {
        SetupDataFetcherWithInvalidAttributeResponse();

        var dto = await _bookmakerDetailsProvider.GetDataAsync([DefaultLanguageCode]);

        Assert.NotNull(dto);
        Assert.Equal(6908, dto.Id);
        Assert.Equal("/unifiedfeed/6908", dto.VirtualHost);
        Assert.Null(dto.ResponseCode);
    }

    [Fact]
    public async Task GetDataAsyncWhenWrongResponseThenThrow()
    {
        SetupDataFetcherWithInvalidResponse();

        await Assert.ThrowsAsync<DeserializationException>(async () => await _bookmakerDetailsProvider.GetDataAsync([DefaultLanguageCode]));
    }

    [Fact]
    public async Task GetDataAsyncWithLanguageCodeWhenCorrectResponseThenDataIsReturned()
    {
        SetupDataFetcherWithValidResponse();

        var dto = await _bookmakerDetailsProvider.GetDataAsync(DefaultLanguageCode);

        Assert.NotNull(dto);
        Assert.Equal(6908, dto.Id);
        Assert.Equal("/unifiedfeed/6908", dto.VirtualHost);
        Assert.Equal(HttpStatusCode.OK, dto.ResponseCode);
    }

    [Fact]
    public async Task GetDataAsyncWithLanguageCodeWhenCorrectResponseThenDefaultServerDifferenceIsZero()
    {
        SetupDataFetcherWithValidResponse();

        var dto = await _bookmakerDetailsProvider.GetDataAsync(DefaultLanguageCode);

        Assert.NotNull(dto);
        Assert.Equal(TimeSpan.Zero, dto.ServerTimeDifference);
    }

    [Fact]
    public async Task GetDataAsyncWithLanguageCodeWhenWrongResponseThenReturnsValidAttributeData()
    {
        SetupDataFetcherWithInvalidAttributeResponse();

        var dto = await _bookmakerDetailsProvider.GetDataAsync(DefaultLanguageCode);

        Assert.NotNull(dto);
        Assert.Equal(6908, dto.Id);
        Assert.Equal("/unifiedfeed/6908", dto.VirtualHost);
        Assert.Null(dto.ResponseCode);
    }

    [Fact]
    public async Task GetDataAsyncWithLanguageCodeWhenWrongResponseThenThrow()
    {
        SetupDataFetcherWithInvalidResponse();

        await Assert.ThrowsAsync<DeserializationException>(async () => await _bookmakerDetailsProvider.GetDataAsync(DefaultLanguageCode));
    }

    [Fact]
    public void GetDataWhenIdentifierEmptyThenReturnData()
    {
        SetupDataFetcherWithValidResponse();

        var dto = _bookmakerDetailsProvider.GetData(Array.Empty<string>());

        Assert.NotNull(dto);
    }

    [Fact]
    public void GetDataWhenCorrectResponseThenDataIsReturned()
    {
        SetupDataFetcherWithValidResponse();

        var dto = _bookmakerDetailsProvider.GetData([DefaultLanguageCode]);

        Assert.NotNull(dto);
        Assert.Equal(6908, dto.Id);
        Assert.Equal("/unifiedfeed/6908", dto.VirtualHost);
        Assert.Equal(HttpStatusCode.OK, dto.ResponseCode);
    }

    [Fact]
    public void GetDataWhenCorrectResponseThenDefaultServerDifferenceIsZero()
    {
        SetupDataFetcherWithValidResponse();

        var dto = _bookmakerDetailsProvider.GetData([DefaultLanguageCode]);

        Assert.NotNull(dto);
        Assert.Equal(TimeSpan.Zero, dto.ServerTimeDifference);
    }

    [Fact]
    public void GetDataWhenWrongResponseThenReturnsValidAttributeData()
    {
        SetupDataFetcherWithInvalidAttributeResponse();

        var dto = _bookmakerDetailsProvider.GetData([DefaultLanguageCode]);

        Assert.NotNull(dto);
        Assert.Equal(6908, dto.Id);
        Assert.Equal("/unifiedfeed/6908", dto.VirtualHost);
        Assert.Null(dto.ResponseCode);
    }

    [Fact]
    public void GetDataWhenWrongResponseThenThrow()
    {
        SetupDataFetcherWithInvalidResponse();

        Assert.Throws<DeserializationException>(() => _bookmakerDetailsProvider.GetData([DefaultLanguageCode]));
    }

    [Fact]
    public void GetDataWithLanguageCodeWhenCorrectResponseThenDataIsReturned()
    {
        SetupDataFetcherWithValidResponse();

        var dto = _bookmakerDetailsProvider.GetData(DefaultLanguageCode);

        Assert.NotNull(dto);
        Assert.Equal(6908, dto.Id);
        Assert.Equal("/unifiedfeed/6908", dto.VirtualHost);
        Assert.Equal(HttpStatusCode.OK, dto.ResponseCode);
    }

    [Fact]
    public void GetDataWithLanguageCodeWhenCorrectResponseThenDefaultServerDifferenceIsZero()
    {
        SetupDataFetcherWithValidResponse();

        var dto = _bookmakerDetailsProvider.GetData(DefaultLanguageCode);

        Assert.NotNull(dto);
        Assert.Equal(TimeSpan.Zero, dto.ServerTimeDifference);
    }

    [Fact]
    public void GetDataWithLanguageCodeWhenWrongResponseThenReturnsValidAttributeData()
    {
        SetupDataFetcherWithInvalidAttributeResponse();

        var dto = _bookmakerDetailsProvider.GetData(DefaultLanguageCode);

        Assert.NotNull(dto);
        Assert.Equal(6908, dto.Id);
        Assert.Equal("/unifiedfeed/6908", dto.VirtualHost);
        Assert.Null(dto.ResponseCode);
    }

    [Fact]
    public void GetDataWithLanguageCodeWhenWrongResponseThenThrow()
    {
        SetupDataFetcherWithInvalidResponse();

        Assert.Throws<DeserializationException>(() => _bookmakerDetailsProvider.GetData(DefaultLanguageCode));
    }

    [Fact]
    public async Task GetDataAsyncThenRawDataIsInvoked()
    {
        RawApiDataEventArgs receivedArgs = null;
        SetupDataFetcherWithValidResponse();
        _bookmakerDetailsProvider.RawApiDataReceived += (_, args) =>
        {
            Assert.NotNull(args);
            receivedArgs = args;
        };

        await _bookmakerDetailsProvider.GetDataAsync([DefaultLanguageCode]);

        ValidateReceivedRawArgs(receivedArgs);
    }

    [Fact]
    public void GetDataThenRawDataIsInvoked()
    {
        RawApiDataEventArgs receivedArgs = null;
        SetupDataFetcherWithValidResponse();
        _bookmakerDetailsProvider.RawApiDataReceived += (_, args) =>
        {
            Assert.NotNull(args);
            receivedArgs = args;
        };

        _ = _bookmakerDetailsProvider.GetData([DefaultLanguageCode]);

        ValidateReceivedRawArgs(receivedArgs);
    }

    [Fact]
    public async Task GetDataAsyncWhenProcessingRawDataFailsThenDataIsStillReturned()
    {
        RawApiDataEventArgs receivedArgs = null;
        SetupDataFetcherWithValidResponse();
        _bookmakerDetailsProvider.RawApiDataReceived += (_, args) =>
        {
            receivedArgs = args;
            throw new InvalidOperationException("Test exception");
        };

        var dto = await _bookmakerDetailsProvider.GetDataAsync([DefaultLanguageCode]);

        Assert.NotNull(dto);
        ValidateReceivedRawArgs(receivedArgs);
    }

    [Fact]
    public void GetDataWhenProcessingRawDataFailsThenDataIsStillReturned()
    {
        RawApiDataEventArgs receivedArgs = null;
        SetupDataFetcherWithValidResponse();
        _bookmakerDetailsProvider.RawApiDataReceived += (_, args) =>
        {
            receivedArgs = args;
            throw new InvalidOperationException("Test exception");
        };

        var dto = _bookmakerDetailsProvider.GetData([DefaultLanguageCode]);

        Assert.NotNull(dto);
        ValidateReceivedRawArgs(receivedArgs);
    }

    private static void ValidateReceivedRawArgs(RawApiDataEventArgs args)
    {
        Assert.NotNull(args);
        Assert.NotNull(args.Uri);
        Assert.False(string.IsNullOrEmpty(args.Language));
        Assert.True(args.RequestTime >= TimeSpan.Zero);
        Assert.NotNull(args.RestMessage);
        Assert.IsAssignableFrom<bookmaker_details>(args.RestMessage);
    }

    private void SetupDataFetcherWithValidResponse()
    {
        var validWhoamiResponse = "<?xml version='1.0' encoding='UTF-8' standalone='yes'?><bookmaker_details response_code='OK' expire_at='2019-01-03T15:16:30Z' bookmaker_id='6908' virtual_host='/unifiedfeed/6908'/>";
        _dataFetcher.Setup(s => s.GetDataAsync(It.IsAny<Uri>())).ReturnsAsync(FileHelper.GetStreamFromString(validWhoamiResponse));
        _dataFetcher.Setup(s => s.GetData(It.IsAny<Uri>())).Returns(FileHelper.GetStreamFromString(validWhoamiResponse));
    }

    private void SetupDataFetcherWithInvalidAttributeResponse()
    {
        var invalidWhoamiResponse = "<?xml version='1.0' encoding='UTF-8' standalone='yes'?><bookmaker_details responseCode='OK' expire_at='2019-01-03T15:16:30Z' bookmaker_id='6908' virtual_host='/unifiedfeed/6908'/>";
        _dataFetcher.Setup(s => s.GetDataAsync(It.IsAny<Uri>())).ReturnsAsync(FileHelper.GetStreamFromString(invalidWhoamiResponse));
        _dataFetcher.Setup(s => s.GetData(It.IsAny<Uri>())).Returns(FileHelper.GetStreamFromString(invalidWhoamiResponse));
    }

    private void SetupDataFetcherWithInvalidResponse()
    {
        var invalidWhoamiResponse = "<?xml version='1.0' encoding='UTF-8' standalone='yes'?><bookmaker response_code='OK' expire_at='2019-01-03T15:16:30Z' bookmaker_id='6908' virtual_host='/unifiedfeed/6908'/>";
        _dataFetcher.Setup(s => s.GetDataAsync(It.IsAny<Uri>())).ReturnsAsync(FileHelper.GetStreamFromString(invalidWhoamiResponse));
        _dataFetcher.Setup(s => s.GetData(It.IsAny<Uri>())).Returns(FileHelper.GetStreamFromString(invalidWhoamiResponse));
    }
}
