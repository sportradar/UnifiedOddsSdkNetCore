/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Defines a contract implemented by classes capable of dispatching entities
    /// </summary>
    /// <seealso cref="Sportradar.OddsFeed.SDK.API.Internal.IEntityDispatcherInternal" />
    internal interface ISpecificEntityDispatcherInternal : IEntityDispatcherInternal
    {
        /// <summary>
        /// Occurs after the current instance is closed
        /// </summary>
        event EventHandler<EventArgs> OnClosed;
    }
}