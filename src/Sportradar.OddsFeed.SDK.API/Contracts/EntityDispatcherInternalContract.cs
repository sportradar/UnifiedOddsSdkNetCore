/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.API.Contracts
{
    [ContractClassFor(typeof(IEntityDispatcherInternal))]
    internal abstract class EntityDispatcherInternalContract : IEntityDispatcherInternal
    {
        public bool IsOpened => Contract.Result<bool>();

        public void Open()
        {
            // defined in IOpenableContract
            //Contract.Ensures(IsOpened);
        }

        public void Close()
        {
            // defined in IOpenableContract
            //Contract.Ensures(!IsOpened);
        }

        public void Dispatch(FeedMessage message, byte[] rawMessage)
        {
            Contract.Requires(message != null);
        }
    }
}
