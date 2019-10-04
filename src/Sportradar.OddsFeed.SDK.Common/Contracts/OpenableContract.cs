/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;

namespace Sportradar.OddsFeed.SDK.Common.Contracts
{
    [ContractClassFor(typeof(IOpenable))]
    internal abstract class OpenableContract : IOpenable
    {
        public bool IsOpened
        {
            [Pure]
            get
            {
                return Contract.Result<bool>();
            }
        }

        public void Open()
        {
            Contract.Ensures(IsOpened);
        }

        public void Close()
        {
            Contract.Ensures(!IsOpened);
        }
    }
}
