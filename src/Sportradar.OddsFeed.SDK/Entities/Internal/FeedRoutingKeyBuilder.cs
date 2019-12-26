/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using Dawn;
using System.Linq;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    /// <summary>
    /// Defines builder for routing keys and checks for feed sessions combo validation
    /// </summary>
    public static class FeedRoutingKeyBuilder
    {
        //  hi.-.live.odds_change.5.sr:match.12329150.nodeId (.producerId)
        // Note: there is a dot in event_id
        /// <summary>
        /// Validates input list of message interests and returns list of routing keys combination per interest
        /// </summary>
        /// <param name="interests">The list of all session interests</param>
        /// <param name="nodeId">The node id</param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<string>> GenerateKeys(IEnumerable<MessageInterest> interests, int nodeId = 0)
        {
            Guard.Argument(interests, nameof(interests)).NotNull().NotEmpty();

            var messageInterests = interests as IList<MessageInterest> ?? interests.ToList();
            var sessionKeys = new List<List<string>>(messageInterests.Count);

            ValidateInterestCombination(messageInterests);

            var both = HaveBothLowAndHigh(messageInterests);

            foreach (var interest in messageInterests)
            {
                if (both && interest == MessageInterest.LowPriorityMessages)
                {
                   sessionKeys.Add(GetBaseKeys(interest, nodeId).Union(GetLiveKeys()).ToList());
                   continue;
                }
                sessionKeys.Add(GetBaseKeys(interest, nodeId).Union(GetStandardKeys().Union(GetLiveKeys())).ToList());
            }
            return sessionKeys;
        }

        /// <summary>
        /// Gets the standard keys usually added to all sessions
        /// </summary>
        /// <returns>IEnumerable&lt;System.String&gt;.</returns>
        public static IEnumerable<string> GetStandardKeys()
        {
            return new List<string>
            {
                "-.-.-.product_down.#",
                "-.-.-.snapshot_complete.#"
            };
        }

        /// <summary>
        /// Gets the live keys
        /// </summary>
        /// <returns>IEnumerable&lt;System.String&gt;.</returns>
        public static IEnumerable<string> GetLiveKeys()
        {
            return new List<string>
            {
                "-.-.-.alive.#"
            };
        }

        private static void ValidateInterestCombination(IEnumerable<MessageInterest> interests)
        {
            var messageInterests = interests as IList<MessageInterest> ?? interests.ToList();
            if (!messageInterests.Any())
            {
                throw new ArgumentException("Empty MessageInterest list not allowed.", nameof(interests));
            }
            if (messageInterests.Count == 1)
            {
                return;
            }
            if (messageInterests.Count > 3)
            {
                throw new ArgumentException("Combination of MessageInterests is not supported.", nameof(interests));
            }
            if ((messageInterests.Contains(MessageInterest.LowPriorityMessages) && !messageInterests.Contains(MessageInterest.HighPriorityMessages))
                && !(messageInterests.Contains(MessageInterest.LowPriorityMessages) && messageInterests.Contains(MessageInterest.HighPriorityMessages)))
            {
                throw new ArgumentException("Combination must have both Low and High priority messages", nameof(interests));
            }
        }

        private static bool HaveBothLowAndHigh(IEnumerable<MessageInterest> interests)
        {
            var low = false;
            var high = false;

            foreach (var interest in interests)
            {
                if (interest == MessageInterest.HighPriorityMessages)
                {
                    high = true;
                }
                if (interest == MessageInterest.LowPriorityMessages)
                {
                    low = true;
                }
            }

            return low && high;
        }

        private static IEnumerable<string> GetBaseKeys(MessageInterest interest, int nodeId)
        {
            var keys = new List<string>();
            if (interest == MessageInterest.AllMessages)
            {
                keys = AllMessages().ToList();
            }
            else if (interest == MessageInterest.LiveMessagesOnly)
            {
                keys = LiveMessagesOnly().ToList();
            }
            else if (interest == MessageInterest.PrematchMessagesOnly)
            {
                keys = PrematchMessagesOnly().ToList();
            }
            else if (interest == MessageInterest.HighPriorityMessages)
            {
                keys = HighPriorityMessages().ToList();
            }
            else if (interest == MessageInterest.LowPriorityMessages)
            {
                keys = LowPriorityMessages().ToList();
            }
            else if (interest == MessageInterest.VirtualSportMessages)
            {
                keys = VirtualSportMessages().ToList();
            }
            else if (interest.IsEventSpecific)
            {
                keys = SpecificEventsOnly(interest.Events).ToList();
            }

            if (keys.Any())
            {
                var tmpKeys = new List<string>();
                foreach (var key in keys)
                {
                    tmpKeys.Add($"{key}.-.#");
                    if (nodeId != 0)
                    {
                        tmpKeys.Add($"{key}.{nodeId}.#");
                    }
                }
                keys = tmpKeys;
            }

            if (!keys.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(interest), "Unknown MessageInterest.");
            }
            return keys;
        }

        /// <summary>
        /// Constructs a <see cref="MessageInterest"/> indicating an interest in all messages
        /// </summary>
        /// <returns>A <see cref="MessageInterest"/> indicating an interest in all messages</returns>
        private static IEnumerable<string> AllMessages()
        {
            return new[]
            {
                //$"*.*.*.{odds_change.MessageName}.*.*.*",
                //$"*.*.*.{bet_stop.MessageName}.*.*.*",
                //$"*.*.*.{bet_settlement.MessageName}.*.*.*",
                //$"*.*.*.{rollback_bet_settlement.MessageName}.*.*.*",
                //$"*.*.*.{bet_cancel.MessageName}.*.*.*",
                //$"*.*.*.{rollback_bet_cancel.MessageName}.*.*.*",
                //$"*.*.*.{fixture_change.MessageName}.*.*.*"
                "*.*.*.*.*.*.*"
            };
        }

        /// <summary>
        /// Constructs a <see cref="MessageInterest"/> indicating an interest in live messages
        /// </summary>
        /// <returns>A <see cref="MessageInterest"/> indicating an interest in live messages</returns>
        private static IEnumerable<string> LiveMessagesOnly()
        {
            return new[]
            {
                //$"*.*.live.{odds_change.MessageName}.*.*.*",
                //$"*.*.live.{bet_stop.MessageName}.*.*.*",
                //$"*.*.live.{bet_settlement.MessageName}.*.*.*",
                //$"*.*.live.{rollback_bet_settlement.MessageName}.*.*.*",
                //$"*.*.live.{bet_cancel.MessageName}.*.*.*",
                //$"*.*.live.{rollback_bet_cancel.MessageName}.*.*.*",
                //$"*.*.live.{fixture_change.MessageName}.*.*.*"
                "*.*.live.*.*.*.*"
            };
        }

        /// <summary>
        /// Constructs a <see cref="MessageInterest"/> indicating an interest in pre-match messages
        /// </summary>
        /// <returns>A <see cref="MessageInterest"/> indicating an interest in pre-match messages</returns>
        private static IEnumerable<string> PrematchMessagesOnly()
        {
            return new[]
            {
                //$"*.pre.*.{odds_change.MessageName}.*.*.*",
                //$"*.pre.*.{bet_stop.MessageName}.*.*.*",
                //$"*.pre.*.{bet_settlement.MessageName}.*.*.*",
                //$"*.pre.*.{rollback_bet_settlement.MessageName}.*.*.*",
                //$"*.pre.*.{bet_cancel.MessageName}.*.*.*",
                //$"*.pre.*.{rollback_bet_cancel.MessageName}.*.*.*",
                //$"*.pre.*.{fixture_change.MessageName}.*.*.*"
                "*.pre.*.*.*.*.*"
            };
        }

        /// <summary>
        /// Constructs a <see cref="MessageInterest"/> indicating an interest in hi priority messages
        /// </summary>
        /// <returns>A <see cref="MessageInterest"/> indicating an interest in high priority messages</returns>
        private static IEnumerable<string> HighPriorityMessages()
        {
            return new[]
            {
                "hi.*.*.*.*.*.*"
            };
        }

        /// <summary>
        /// Constructs a <see cref="MessageInterest"/> indicating an interest in low priority messages
        /// </summary>
        /// <returns>A <see cref="MessageInterest"/> indicating an interest in low priority messages</returns>
        private static IEnumerable<string> LowPriorityMessages()
        {
            return new[]
            {
                "lo.*.*.*.*.*.*"
            };
        }

        /// <summary>
        /// Constructs a <see cref="MessageInterest"/> indicating an interest in low priority messages
        /// </summary>
        /// <returns>A <see cref="MessageInterest"/> indicating an interest in low priority messages</returns>
        private static IEnumerable<string> VirtualSportMessages()
        {
            return new[]
            {
                "*.virt.*.*.*.*.*",
                "*.*.virt.*.*.*.*"
            };
        }


        /// <summary>
        /// Constructs a <see cref="MessageInterest"/> indicating an interest in messages associated with specific events
        /// </summary>
        /// <param name="eventIds">A <see cref="IEnumerable{Integer}"/> specifying the target events</param>
        /// <returns>A <see cref="MessageInterest"/> indicating an interest in messages associated with specific events</returns>
        private static IEnumerable<string> SpecificEventsOnly(IEnumerable<URN> eventIds)
        {
            Guard.Argument(eventIds, nameof(eventIds)).NotNull().NotEmpty();

            //channels using this routing key will also receive 'system' messages so they have to be manually removed in the receiver
            return eventIds.Select(u => $"#.{u.Prefix}:{u.Type}.{u.Id}");
        }
    }
}
