/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities;

namespace Sportradar.OddsFeed.SDK.API.EventArguments
{
    /// <summary>
    /// Event arguments for the FeedClose events
    /// </summary>
    public class FeedCloseEventArgs : EventArgs
    {
        /// <summary>
        /// A reason why must feed be closed
        /// </summary>
        private readonly string _reason;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedCloseEventArgs"/> class
        /// </summary>
        /// <param name="reason">A reason why feed must be closed</param>
        internal FeedCloseEventArgs(string reason)
        {
            Guard.Argument(reason).NotNull().NotEmpty();

            _reason = reason;
        }

        /// <summary>
        /// Gets a <see cref="IProducerStatusChange"/> implementation containing information about the producer status change
        /// </summary>
        /// <returns>Returns a <see cref="IProducerStatusChange"/> implementation containing information about the producer status change</returns>
        public string GetReason()
        {
            return _reason;
        }
    }
}
