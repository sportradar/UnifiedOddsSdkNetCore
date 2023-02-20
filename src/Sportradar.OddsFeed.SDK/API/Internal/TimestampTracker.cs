/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Class used to track the timestamp and times of incoming messages in order to determine
    /// whether the feed is producing alive messages in a specified interval and if the user is
    /// processing messages in a timely matter.
    /// </summary>
    internal class TimestampTracker : ITimestampTracker
    {
        /// <summary>
        /// A <see cref="ILogger"/>
        /// </summary>
        private static readonly ILogger ExecutionLog = SdkLoggerFactory.GetLoggerForExecution(typeof(TimestampTracker));

        /// <summary>
        /// The max interval in seconds between live messages on a system session
        /// </summary>
        private readonly int _maxInactivitySeconds;

        /// <summary>
        /// The maximum latency of the user messages
        /// </summary>
        private readonly int _maxMessageAgeInSeconds;

        /// <summary>
        /// A <see cref="IReadOnlyDictionary{TKey,TValue}"/> containing timing info last alive message received on the users sessions
        /// </summary>
        private readonly IReadOnlyDictionary<MessageInterest, MessageTimingInfo> _aliveMessagesTimingInfo;

        /// <summary>
        /// A <see cref="IReadOnlyDictionary{TKey,TValue}"/> containing timing info last non-alive message received on the users sessions
        /// </summary>
        private readonly IReadOnlyDictionary<MessageInterest, MessageTimingInfo> _nonAliveMessagesTimingInfo;

        /// <summary>
        /// A <see cref="MessageTimingInfo"/> used to track alive messages from the system session
        /// </summary>
        private readonly MessageTimingInfo _systemAliveTimingInfo;

        /// <summary>
        /// Sdk start time (used for recovery initiation)
        /// </summary>
        public DateTime SdkStartTime { get; }

        /// <summary>
        /// Gets a value indicating whether the feed messages are processed in a timely manner
        /// </summary>
        public bool IsBehind
        {
            get
            {
                if (!_aliveMessagesTimingInfo.Values.All(info => info.Age.TotalSeconds < _maxMessageAgeInSeconds))
                {
                    return true;
                }
                return !_nonAliveMessagesTimingInfo.Values.All(info => info.Latency.TotalSeconds < _maxMessageAgeInSeconds);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the alive messages on a system session are received in a timely manner
        /// </summary>
        public bool IsAliveViolated => _systemAliveTimingInfo.Age.TotalSeconds > _maxInactivitySeconds;

        /// <summary>
        /// Gets the timestamp of the oldest (the one that was generated first) alive message received on the user session.
        /// </summary>
        public long OldestUserAliveTimestamp => _aliveMessagesTimingInfo.Count == 0
            ? SdkInfo.ToEpochTime(TimeProviderAccessor.Current.Now)
            : SdkInfo.ToEpochTime(_aliveMessagesTimingInfo.Values.Min(t => t.GeneratedAt));

        /// <summary>
        /// Gets the epoch timestamp specifying when the last system alive message received was generated
        /// </summary>
        public long SystemAliveTimestamp => SdkInfo.ToEpochTime(_systemAliveTimingInfo.GeneratedAt);

        /// <summary>
        /// Initializes a new instance of the <see cref="TimestampTracker"/> class
        /// </summary>
        /// <param name="producer">The <see cref="Producer"/> associated with the constructed instance</param>
        /// <param name="allInterests">A <see cref="IEnumerable{T}"/> containing <see cref="MessageInterest"/> for all created sessions</param>
        /// <param name="maxInactivitySeconds">The max interval in seconds between live messages on a system session</param>
        /// <param name="maxMessageAgeInSeconds">The maximum latency of the user messages</param>
        public TimestampTracker(Producer producer, ICollection<MessageInterest> allInterests, int maxInactivitySeconds, int maxMessageAgeInSeconds)
        {
            Guard.Argument(producer, nameof(producer)).NotNull();
            Guard.Argument(maxInactivitySeconds, nameof(maxInactivitySeconds)).Positive();
            Guard.Argument(maxMessageAgeInSeconds, nameof(maxInactivitySeconds)).Positive();
            Guard.Argument(allInterests, nameof(allInterests)).NotNull().NotEmpty();

            _maxInactivitySeconds = maxInactivitySeconds;
            _maxMessageAgeInSeconds = maxMessageAgeInSeconds;
            _systemAliveTimingInfo = new MessageTimingInfo(SdkInfo.ToEpochTime(TimeProviderAccessor.Current.Now));

            var aliveMessagesTimingInfo = new Dictionary<MessageInterest, MessageTimingInfo>();
            var nonAliveMessagesTimingInfo = new Dictionary<MessageInterest, MessageTimingInfo>();

            var producerScopes = producer.Scope.Select(MessageInterest.FromScope).ToList();
            foreach (var interest in allInterests)
            {
                if (!interest.IsScopeInterest || producerScopes.Contains(interest))
                {
                    aliveMessagesTimingInfo.Add(interest, new MessageTimingInfo(SdkInfo.ToEpochTime(TimeProviderAccessor.Current.Now)));
                    nonAliveMessagesTimingInfo.Add(interest, new MessageTimingInfo(SdkInfo.ToEpochTime(TimeProviderAccessor.Current.Now)));
                }
            }
            _aliveMessagesTimingInfo = new ReadOnlyDictionary<MessageInterest, MessageTimingInfo>(aliveMessagesTimingInfo);
            _nonAliveMessagesTimingInfo = new ReadOnlyDictionary<MessageInterest, MessageTimingInfo>(nonAliveMessagesTimingInfo);
            SdkStartTime = DateTime.Now;
        }

        /// <summary>
        /// Updates the timing info in the provided dictionary
        /// </summary>
        /// <param name="dictionary">The <see cref="IReadOnlyDictionary{TKey,TValue}"/> to modify</param>
        /// <param name="interest">The <see cref="MessageInterest"/> associated with the session which received the message</param>
        /// <param name="message">The received <see cref="FeedMessage"/></param>
        private static void UpdateTimingInfo(IReadOnlyDictionary<MessageInterest, MessageTimingInfo> dictionary, MessageInterest interest, FeedMessage message)
        {
            Guard.Argument(dictionary, nameof(dictionary)).NotNull();
            Guard.Argument(interest, nameof(interest)).NotNull();
            Guard.Argument(message, nameof(message)).NotNull();

            if (dictionary.TryGetValue(interest, out var timingInfo))
            {
                timingInfo.Update(message.GeneratedAt);
            }
            else
            {
                if (!(message is alive))
                {
                    ExecutionLog.LogError($"Message timing info for message type:{message.GetType().Name} and interest:{interest.Name} does not exist in this scope:{string.Join(",", dictionary.Keys)}.");
                }
            }
        }

        /// <summary>
        /// Updates the timing info in the provided dictionary if the latency on the existing entry is bigger than on the new one
        /// </summary>
        /// <param name="dictionary">The <see cref="IReadOnlyDictionary{TKey,TValue}"/> to modify</param>
        /// <param name="interest">The <see cref="MessageInterest"/> associated with the session which received the message</param>
        /// <param name="message">The received <see cref="FeedMessage"/></param>
        private static void UpdateTimingInfoIfLatencyLower(IReadOnlyDictionary<MessageInterest, MessageTimingInfo> dictionary, MessageInterest interest, FeedMessage message)
        {
            Guard.Argument(dictionary, nameof(dictionary)).NotNull();
            Guard.Argument(interest, nameof(interest)).NotNull();
            Guard.Argument(message, nameof(message)).NotNull();

            if (dictionary.TryGetValue(interest, out var timingInfo))
            {
                var messageDateTime = SdkInfo.FromEpochTime(message.GeneratedAt);
                var newLatency = TimeProviderAccessor.Current.Now <= messageDateTime
                    ? TimeSpan.Zero
                    : TimeProviderAccessor.Current.Now - messageDateTime;
                if (timingInfo.Latency > newLatency)
                {
                    timingInfo.Update(message.GeneratedAt);
                }
            }
            else
            {
                if (!(message is alive))
                {
                    ExecutionLog.LogError($"Message timing info for message type:{message.GetType().Name}, interest:{interest.Name} does not exist.");
                }
            }
        }

        /// <summary>
        /// Records the provided <see cref="FeedMessage"/> timing info
        /// </summary>
        /// <param name="interest">The <see cref="MessageInterest"/> associated with the session receiving the alive message</param>
        /// <param name="message">The received <see cref="FeedMessage"/></param>
        public void ProcessUserMessage(MessageInterest interest, FeedMessage message)
        {
            if (message is odds_change || message is bet_stop)
            {
                UpdateTimingInfo(_nonAliveMessagesTimingInfo, interest, message);
            }
            else if (message is alive)
            {
                UpdateTimingInfo(_aliveMessagesTimingInfo, interest, message);
                UpdateTimingInfoIfLatencyLower(_nonAliveMessagesTimingInfo, interest, message);
            }
        }

        /// <summary>
        /// Records the provided <see cref="alive"/> message timing info received on the system session
        /// </summary>
        /// <param name="alive">The <see cref="alive"/> message received on a system session</param>
        public void ProcessSystemAlive(alive alive)
        {
            _systemAliveTimingInfo.Update(alive.timestamp);
        }

        /// <summary>
        /// Constructs and returns a <see cref="string"/> representation of the current instance
        /// </summary>
        /// <returns>a <see cref="string"/> representation of the current instance</returns>
        public override string ToString()
        {
            var aliveInfo = string.Join(",", _aliveMessagesTimingInfo.Select(pair => pair.Key.Name + ":" + "age=" + $"{pair.Value.Age.Hours:D2}:{pair.Value.Age.Minutes:D2}:{pair.Value.Age.Seconds:D2}.{pair.Value.Age.Milliseconds:D3}"));
            var nonAliveInfo = string.Join(",", _nonAliveMessagesTimingInfo.Select(pair => pair.Key.Name + ":" + "latency=" + $"{pair.Value.Latency.Hours:D2}:{pair.Value.Latency.Minutes:D2}:{pair.Value.Latency.Seconds:D2}.{pair.Value.Latency.Milliseconds:D3}"));
            var behindInfo = $"IsBehind({IsBehind}):Alive(s)[{aliveInfo}],NonAlives[{nonAliveInfo}]";
            var pingStatus = IsAliveViolated ? "Failed" : "Ok";
            var violationInfo = $"Ping({pingStatus}):age={_systemAliveTimingInfo.Age.Hours:D2}:{_systemAliveTimingInfo.Age.Minutes:D2}:{_systemAliveTimingInfo.Age.Seconds:D2}.{_systemAliveTimingInfo.Age.Milliseconds:D3}";
            return $"[[{violationInfo}],[{behindInfo}]]";
        }

        /// <summary>
        /// Holds timing related info for a message received from the feed
        /// </summary>
        /// <remarks>
        /// This class is not thread safe
        /// </remarks>
        class MessageTimingInfo
        {
            /// <summary>
            /// Gets a <see cref="DateTime"/> specifying when the associated message was generated (on the server side)
            /// </summary>
            public DateTime GeneratedAt { get; private set; }

            /// <summary>
            /// Gets <see cref="TimeSpan"/> specifying the latency between message generate and receive time
            /// </summary>
            public TimeSpan Latency { get; private set; }

            /// <summary>
            /// Gets a <see cref="TimeSpan"/> specifying how long ago the message was received
            /// </summary>
            public TimeSpan Age => TimeProviderAccessor.Current.Now - GeneratedAt;

            /// <summary>
            /// Initialize a new instance of the <see cref="MessageTimingInfo"/> class
            /// </summary>
            /// <param name="generatedAt">An epoch timestamp specifying when the message was generated</param>
            public MessageTimingInfo(long generatedAt)
            {
                Update(generatedAt);
            }

            /// <summary>
            /// Updates the information held by the current instance
            /// </summary>
            /// <param name="generatedAt">Epoch time of the message generation</param>
            public void Update(long generatedAt)
            {
                var currentTime = TimeProviderAccessor.Current.Now;
                GeneratedAt = SdkInfo.FromEpochTime(generatedAt);
                Latency = GeneratedAt >= currentTime
                    ? TimeSpan.Zero
                    : currentTime - GeneratedAt;
            }
        }
    }
}
