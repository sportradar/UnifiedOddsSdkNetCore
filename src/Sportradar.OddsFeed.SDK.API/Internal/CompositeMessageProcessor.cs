/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Internal.EventArguments;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// A <see cref="IFeedMessageProcessor"/> implementation used to control the flow of messages throw
    /// message processors it control.
    /// </summary>
    internal class CompositeMessageProcessor : MessageProcessorBase, IFeedMessageProcessor
    {
        private readonly IReadOnlyList<IFeedMessageProcessor> _processors;

        /// <summary>
        /// The processor identifier
        /// </summary>
        /// <value>The processor identifier.</value>
        public string ProcessorId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeMessageProcessor"/> class.
        /// </summary>
        /// <param name="processors">The list of processors.</param>
        public CompositeMessageProcessor(IEnumerable<IFeedMessageProcessor> processors)
        {
            Contract.Requires(processors != null);
            Contract.Requires(processors.Any());
            Contract.Requires(processors.All(p => p != null));

            ProcessorId = Guid.NewGuid().ToString().Substring(0,4);

            _processors = processors as IReadOnlyList<IFeedMessageProcessor>;
            Contract.Assume(_processors != null);

            foreach (var processor in _processors)
            {
                //Debug.WriteLine($"{ProcessorId} - CompositeMessageProcessor has processor {processor.ProcessorId}");
                processor.MessageProcessed += OnProcessorMessageProcessedEvent;
            }
        }

        /// <summary>
        /// Defined field invariants needed by code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_processors != null);
            Contract.Invariant(_processors.Any());
            Contract.Invariant(_processors.All(p => p != null));
        }

        private void OnProcessorMessageProcessedEvent(object sender, FeedMessageReceivedEventArgs e)
        {
            //Debug.WriteLine($"{ProcessorId} - CompositeMessageProcessor.OnProcessorMessageProcessedEvent called.");
            var sendingProcessor = (IFeedMessageProcessor) sender;
            int index;
            for (index = 0; index < _processors.Count; index++)
            {
                if (sendingProcessor == _processors[index])
                {
                    break;
                }
            }

            if (index < _processors.Count - 1)
            {
                _processors[index + 1].ProcessMessage(e.Message, e.Interest, e.RawMessage);
                return;
            }

            //Debug.WriteLine($"{ProcessorId} - CompositeMessageProcessor.OnProcessorMessageProcessedEvent finishing.");

            RaiseOnMessageProcessedEvent(e);
        }

        /// <summary>
        /// Processes and dispatches the provided <see cref="FeedMessage"/> instance
        /// </summary>
        /// <param name="message">A <see cref="FeedMessage"/> instance to be processed</param>
        /// <param name="interest">A <see cref="MessageInterest"/> specifying the interest of the associated session</param>
        /// <param name="rawMessage">A raw message received from the feed</param>
        public void ProcessMessage(FeedMessage message, MessageInterest interest, byte[] rawMessage)
        {
            _processors.First().ProcessMessage(message, interest, rawMessage);
        }
    }
}
