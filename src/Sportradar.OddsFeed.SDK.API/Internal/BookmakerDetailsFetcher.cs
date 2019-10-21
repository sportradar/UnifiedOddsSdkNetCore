/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Dawn;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Internal.Log;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Class used to get the bookmaker details
    /// </summary>
    [Log(LoggerType.ClientInteraction)]
    internal class BookmakerDetailsFetcher : MarshalByRefObject
    {
        private readonly IDataProvider<BookmakerDetailsDTO> _bookmakerDetailsProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="BookmakerDetailsFetcher"/> class
        /// </summary>
        /// <param name="bookmakerDetailsProvider">A <see cref="IDataProvider{BookmakerDetailsDTO}"/> used to get <see cref="BookmakerDetailsDTO"/></param>
        public BookmakerDetailsFetcher(IDataProvider<BookmakerDetailsDTO> bookmakerDetailsProvider)
        {
            Guard.Argument(bookmakerDetailsProvider).NotNull();

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
