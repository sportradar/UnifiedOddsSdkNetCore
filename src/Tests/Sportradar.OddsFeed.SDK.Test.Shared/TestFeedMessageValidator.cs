/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Test.Shared
{
    /// <summary>
    /// A class used for validation of <see cref="FeedMessage"/> instances
    /// </summary>
    public class TestFeedMessageValidator : IFeedMessageValidator
    {
        /// <summary>
        /// Validates the specified <see cref="FeedMessage" /> instance
        /// </summary>
        /// <param name="message">A <see cref="FeedMessage" /> instance to be validated</param>
        /// <returns>Always Success</returns>
        public ValidationResult Validate(FeedMessage message)
        {
            if (!ValidateMessage(message))
            {
                return ValidationResult.FAILURE;
            }

            return ValidationResult.SUCCESS;
        }

        /// <summary>
        /// Validates the basic properties of the provided <see cref="FeedMessage"/>
        /// </summary>
        /// <param name="message">The message to be validated</param>
        /// <returns>True if the validation was successful, otherwise false</returns>
        private static bool ValidateMessage(FeedMessage message)
        {
            Contract.Requires(message != null);

            var result = TestProducerManager.Create().Exists(message.ProducerId);

            if (message.IsEventRelated)
            {
                if (message.SportId == null)
                {
                    return false;
                }
                URN eventUrn;
                if (URN.TryParse(message.EventId, out eventUrn))
                {
                    message.EventURN = eventUrn;
                }
                else
                {
                    result = false;
                }
            }

            if (message.RequestIdUsage == PropertyUsage.REQUIRED && message.RequestId < 1 ||
                message.RequestIdUsage == PropertyUsage.OPTIONAL && message.RequestId.HasValue && message.RequestId.Value < 0)
            {
                result = false;
            }
            return result;
        }
    }
}
