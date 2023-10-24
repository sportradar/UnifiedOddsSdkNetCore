using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Api;

public abstract class BookmakerDetailsFetcherTests : AutoMockerUnitTest
{
    private readonly BookmakerDetailsFetcher _sut;

    private readonly bookmaker_details _sourceDetails = new bookmaker_details
    {
        bookmaker_id = 123,
        message = "test message",
        response_codeSpecified = true,
        response_code = response_code.OK
    };

    protected BookmakerDetailsFetcherTests()
    {
        _sut = Mocker.CreateInstance<BookmakerDetailsFetcher>();
    }

    public class WhenGetBookmakerDetailsIsSuccessful : BookmakerDetailsFetcherTests
    {
        public WhenGetBookmakerDetailsIsSuccessful()
        {
            Mocker.GetMock<IDataProvider<BookmakerDetailsDto>>()
                .Setup(x => x.GetDataAsync(new string[1]))
                .ReturnsAsync(new BookmakerDetailsDto(_sourceDetails, TimeSpan.Zero));
        }

        [Fact]
        public async Task Then_it_returns_bookmaker_details()
        {
            var bookmakerDetails = await _sut.WhoAmIAsync();

            using (new AssertionScope())
            {
                bookmakerDetails.BookmakerId.Should().Be(_sourceDetails.bookmaker_id);
                bookmakerDetails.Message.Should().Be(_sourceDetails.message);
                bookmakerDetails.ResponseCode.Should().Be(HttpStatusCode.OK);
            }
        }
    }
}
