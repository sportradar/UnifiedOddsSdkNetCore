/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    /// <summary>
    /// Defines a contract implemented by classes used for validation of feed(AMQP) messages
    /// </summary>
    public interface IFeedMessageValidator
    {
        /// <summary>
        /// Validates the specified <see cref="FeedMessage"/> instance
        /// </summary>
        /// <param name="message">A <see cref="FeedMessage"/> instance to be validated.</param>
        /// <returns>A <see cref="ValidationResult"/> specifying the validation result.</returns>
        ValidationResult Validate(FeedMessage message);
    }
}
