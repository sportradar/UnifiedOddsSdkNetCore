/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common;
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
        /// <summary>
        /// The execution log
        /// </summary>
        private static readonly ILogger ExecutionLog = SdkLoggerFactory.GetLoggerForExecution(typeof(CompositeMessageProcessor));

        private readonly IReadOnlyList<IFeedMessageProcessor> _processors;

        /// <summary>
        /// The processor identifier
        /// </summary>
        /// <value>The processor identifier.</value>
        public string ProcessorId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeMessageProcessor"/> class
        /// </summary>
        /// <param name="processors">The list of processors.</param>
        public CompositeMessageProcessor(List<IFeedMessageProcessor> processors)
        {
            Guard.Argument(processors, nameof(processors)).NotNull().NotEmpty().Require(processors.All(p => p != null));

            ProcessorId = Guid.NewGuid().ToString().Substring(0, 4);

            _processors = processors;

            foreach (var processor in _processors)
            {
                processor.MessageProcessed += OnProcessorMessageProcessedEvent;
            }
        }

        private void OnProcessorMessageProcessedEvent(object sender, FeedMessageReceivedEventArgs e)
        {
            ExecutionLog.LogDebug($"{ProcessorId} - CompositeMessageProcessor.OnProcessorMessageProcessedEvent called by {sender?.GetType().Name}");
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
            Guard.Argument(message, nameof(message)).NotNull();
            Guard.Argument(interest, nameof(interest)).NotNull();

            foreach (var processor in _processors)
            {
                processor.ProcessMessage(message, interest, rawMessage);
            }
            RaiseOnMessageProcessedEvent(new FeedMessageReceivedEventArgs(message, interest, rawMessage));
        }
    }
}
