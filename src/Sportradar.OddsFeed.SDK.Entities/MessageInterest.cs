/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Defines which messages will be provided by feed
    /// </summary>
    public class MessageInterest
    {
        /// <summary>
        /// Gets the name of the message interest
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the routing key used to select appropriate AMQP exchange
        /// </summary>
        public int ProducerId { get; }

        /// <summary>
        /// Gets the value indicating whether the current interest is a scope interest
        /// (live, prematch or virt)
        /// </summary>
        public bool IsScopeInterest => MessageScopes.Contains(this);

        /// <summary>
        /// The list of events (only used with <see cref="SpecificEventsOnly"/> type)
        /// </summary>
        internal readonly IEnumerable<URN> Events;

        /// <summary>
        /// Gets a value indicating whether the current instance is related to only specific events
        /// </summary>
        internal bool IsEventSpecific => Events != null;

        /// <summary>
        /// Gets the scopes that specific message interest covers
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        internal string Scope { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageInterest"/> class
        /// </summary>
        private MessageInterest(string name, int producerId = -1, IEnumerable<URN> events = null, string scopes = "live|prematch|virt")
        {
            Name = name;
            ProducerId = producerId;
            Events = events;
            Scope = scopes;
        }

        /// <summary>
        /// Constructs a <see cref="string"/> representation of the current instance
        /// </summary>
        /// <returns>Returns current instance represented as string</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Constructs a <see cref="MessageInterest"/> indicating an interest in all messages
        /// </summary>
        /// <returns>A <see cref="MessageInterest"/> indicating an interest in all messages</returns>
        public static readonly MessageInterest AllMessages = new MessageInterest("all");

        /// <summary>
        /// Constructs a <see cref="MessageInterest"/> indicating an interest in live messages
        /// </summary>
        /// <returns>A <see cref="MessageInterest"/> indicating an interest in live messages</returns>
        public static readonly MessageInterest LiveMessagesOnly = new MessageInterest("live", 1, null, "live");       // LO

        /// <summary>
        /// Constructs a <see cref="MessageInterest"/> indicating an interest in pre-match messages
        /// </summary>
        /// <returns>A <see cref="MessageInterest"/> indicating an interest in pre-match messages</returns>
        public static readonly MessageInterest PrematchMessagesOnly = new MessageInterest("prematch", 3,  null, "prematch");   //LCOO

        /// <summary>
        /// Constructs a <see cref="MessageInterest"/> indicating an interest in hi priority messages
        /// </summary>
        /// <returns>A <see cref="MessageInterest"/> indicating an interest in high priority messages</returns>
        public static readonly MessageInterest HighPriorityMessages = new MessageInterest("high_priority");

        /// <summary>
        /// Constructs a <see cref="MessageInterest"/> indicating an interest in low priority messages
        /// </summary>
        /// <returns>A <see cref="MessageInterest"/> indicating an interest in low priority messages</returns>
        public static readonly MessageInterest LowPriorityMessages = new MessageInterest("low_priority");

        /// <summary>
        /// Constructs a <see cref="MessageInterest"/> indicating an interest in messages for virtual sports
        /// </summary>
        /// <returns>A <see cref="MessageInterest"/> indicating an interest in messages for virtual sports</returns>
        public static readonly MessageInterest VirtualSportMessages = new MessageInterest("virtual", -1, null, "virt");

        /// <summary>
        /// Constructs a <see cref="MessageInterest"/> indicating an interest in messages associated with specific events
        /// </summary>
        /// <param name="eventIds">A <see cref="IEnumerable{Integer}"/> specifying the target events</param>
        /// <returns>A <see cref="MessageInterest"/> indicating an interest in messages associated with specific events</returns>
        public static MessageInterest SpecificEventsOnly(IEnumerable<URN> eventIds)
        {
            Contract.Requires(eventIds != null);
            Contract.Requires(eventIds.Any());

            //channels using this routing key will also receive 'system' messages so they have to be manually removed in the receiver
            return new MessageInterest("custom", -1, eventIds.Distinct());
        }

        /// <summary>
        /// List of <see cref="MessageInterest"/> representing defined messages scopes
        /// </summary>
        public static readonly MessageInterest[] MessageScopes = {
            LiveMessagesOnly,
            PrematchMessagesOnly,
            VirtualSportMessages,
        };

        /// <summary>
        /// Gets all available <see cref="MessageInterest"/> instances
        /// </summary>
        public static readonly IEnumerable<MessageInterest> DefinedInterests = new[]
        {
            AllMessages,
            LiveMessagesOnly,
            PrematchMessagesOnly,
            VirtualSportMessages,
            HighPriorityMessages,
            LowPriorityMessages
        };

        /// <summary>
        /// Gets a <see cref="MessageInterest"/> representing a scope specified by it's name.
        /// </summary>
        /// <param name="scopeName">The name of the scope</param>
        /// <returns>The <see cref="MessageInterest"/> representing a scope specified by it's name. </returns>
        public static MessageInterest FromScope(string scopeName)
        {
            Contract.Requires(!string.IsNullOrEmpty(scopeName));
            Contract.Ensures(Contract.Result<MessageInterest>() != null);

            switch (scopeName)
            {
                case "live":
                    return LiveMessagesOnly;
                case "prematch":
                    return PrematchMessagesOnly;
                case "virtual":
                    return VirtualSportMessages;
                default:
                    throw new InvalidOperationException($"{scopeName} is not a valid scope name.");
            }
        }
        /// <summary>
        /// Determines whether the provided list of <see cref="MessageInterest"/> is a valid combination of sessions
        /// </summary>
        /// <param name="interests"></param>
        /// <returns>True if the provided combination is valid. Otherwise false</returns>
        public static bool IsCombinationValid(IEnumerable<MessageInterest> interests)
        {
            if (interests == null)
            {
                throw new ArgumentNullException(nameof(interests));
            }

            var interestList = interests as List<MessageInterest> ?? new List<MessageInterest>(interests);

            return interestList.Count == 1
                   || interestList.Count == 2 && interestList.Contains(HighPriorityMessages) || interestList.Contains(LowPriorityMessages)
                   || interestList.All(MessageScopes.Contains);
        }
    }
}
