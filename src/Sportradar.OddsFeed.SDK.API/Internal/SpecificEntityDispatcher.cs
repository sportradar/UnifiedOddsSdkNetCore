/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// An entity dispatcher capable of dispatching sport specific entities
    /// </summary>
    /// <typeparam name="T">The type of the entities dispatched by the dispatcher</typeparam>
    /// <seealso cref="EntityDispatcher{T}" />
    internal class SpecificEntityDispatcher<T> : EntityDispatcher<T>, ISpecificEntityDispatcher<T>, ISpecificEntityDispatcherInternal where T : ISportEvent
    {

        /// <summary>
        /// Occurs after the current instance is closed
        /// </summary>
        public event EventHandler<EventArgs> OnClosed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecificEntityDispatcher{T}"/> class.
        /// </summary>
        /// <param name="messageMapper">A <see cref="IFeedMessageMapper"/> used to map the feed messages to messages used by the SDK</param>
        /// <param name="defaultCultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the default languages as specified in the configuration</param>
        internal SpecificEntityDispatcher(IFeedMessageMapper messageMapper, IEnumerable<CultureInfo> defaultCultures)
            : base(messageMapper, defaultCultures)
        {

        }

        /// <summary>
        /// When overridden in derived class, it executes steps needed when opening the instance
        /// </summary>
        protected override void OnOpening()
        {
            //nothing to do
        }

        /// <summary>
        /// When overridden in derived class, it executes steps needed when closing the instance
        /// </summary>
        protected override void OnClosing()
        {
            OnClosed?.Invoke(this, new EventArgs());
        }
    }
}
