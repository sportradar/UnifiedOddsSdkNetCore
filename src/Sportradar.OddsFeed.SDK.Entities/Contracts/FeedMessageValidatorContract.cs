/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Entities.Contracts
{
    [ContractClassFor(typeof(IFeedMessageValidator))]
    abstract class FeedMessageValidatorContract : IFeedMessageValidator
    {
        public ValidationResult Validate(FeedMessage message)
        {
            Contract.Requires(message != null);
            return Contract.Result<ValidationResult>();
        }
    }
}
