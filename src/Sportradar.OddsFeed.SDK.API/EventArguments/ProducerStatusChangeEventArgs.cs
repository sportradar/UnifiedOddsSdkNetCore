/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Entities;

namespace Sportradar.OddsFeed.SDK.API.EventArguments
{
    /// <summary>
    /// Event arguments for the <see cref="IOddsFeed.ProducerDown"/> events
    /// </summary>
    public class ProducerStatusChangeEventArgs : EventArgs
    {
        /// <summary>
        /// a <see cref="IProducerStatusChange"/> implementation containing information about the producer whose status has changed
        /// </summary>
        private readonly IProducerStatusChange _statusChange;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProducerStatusChangeEventArgs"/> class
        /// </summary>
        /// <param name="producerStatusChange">a <see cref="IProducerStatusChange"/> implementation containing information about the producer whose status has changed</param>
        internal ProducerStatusChangeEventArgs(IProducerStatusChange producerStatusChange)
        {
            Contract.Requires(producerStatusChange != null);
            _statusChange = producerStatusChange;
        }

        /// <summary>
        /// Gets a <see cref="IProducerStatusChange"/> implementation containing information about the producer status change
        /// </summary>
        /// <returns>Returns a <see cref="IProducerStatusChange"/> implementation containing information about the producer status change</returns>
        public IProducerStatusChange GetProducerStatusChange()
        {
            return _statusChange;
        }
    }
}
