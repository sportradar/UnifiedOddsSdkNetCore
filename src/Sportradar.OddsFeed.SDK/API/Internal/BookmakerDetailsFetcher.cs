/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;

namespace Sportradar.OddsFeed.SDK.Api.Internal
{
    /// <summary>
    /// Class used to get the bookmaker details
    /// </summary>
    internal class BookmakerDetailsFetcher
    {
        private readonly IDataProvider<BookmakerDetailsDto> _bookmakerDetailsProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="BookmakerDetailsFetcher"/> class
        /// </summary>
        /// <param name="bookmakerDetailsProvider">A <see cref="IDataProvider{BookmakerDetailsDto}"/> used to get <see cref="BookmakerDetailsDto"/></param>
        public BookmakerDetailsFetcher(IDataProvider<BookmakerDetailsDto> bookmakerDetailsProvider)
        {
            Guard.Argument(bookmakerDetailsProvider, nameof(bookmakerDetailsProvider)).NotNull();

            _bookmakerDetailsProvider = bookmakerDetailsProvider;
        }

        /// <summary>
        /// Asynchronously gets and returns <see cref="IBookmakerDetails"/> containing details information about the bookmaker
        /// </summary>
        /// <returns>A <see cref="Task{IBookmakerDetails}"/> representing the asynchronous operation</returns>
        public async Task<IBookmakerDetails> WhoAmIAsync()
        {
            var detailsDto = await _bookmakerDetailsProvider.GetDataAsync(new string[1]).ConfigureAwait(false);
            var bookmakerDetails = new BookmakerDetails(detailsDto);
            return bookmakerDetails;
        }
    }
}
