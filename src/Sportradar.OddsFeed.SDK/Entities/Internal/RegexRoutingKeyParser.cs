/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Text.RegularExpressions;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    /// <summary>
    /// A <see cref="IRoutingKeyParser"/> implementation which uses regex.
    /// </summary>
    public class RegexRoutingKeyParser : IRoutingKeyParser
    {
        /// <summary>
        /// Name of the regex group containing sport id
        /// </summary>
        private const string GroupName = "SportId";

        /// <summary>
        /// A prefix of the sport id
        /// </summary>
        private const string SportIdPrefix = "sr:sport:";

        /// <summary>
        /// Regex pattern used to parse the routing key
        /// </summary>
        private static readonly string RegexPatternFormat = @"\A.+\.{0}\." + $@"(?<{GroupName}>\d+)\..+\z";

        /// <summary>
        /// Gets a <see cref="URN"/> representing the sportId by parsing the provided <code>routingKey</code>
        /// </summary>
        /// <param name="routingKey">The routing key specified by the feed</param>
        /// <param name="messageTypeName">The type name of the received message</param>
        /// <returns>The sportId obtained by parsing the provided <code>routingKey</code></returns>
        public URN GetSportId(string routingKey, string messageTypeName)
        {
            var match = Regex.Match(routingKey, string.Format(RegexPatternFormat, messageTypeName));
            if (!match.Success)
            {
                throw new FormatException($"The format of string {routingKey} is not correct");
            }

            return URN.Parse($"{SportIdPrefix + match.Groups[GroupName].Value}");
        }

        /// <summary>
        /// Tries to get a <see cref="URN"/> representing of the sportId by parsing the provided <code>routingKey</code>
        /// </summary>
        /// <param name="routingKey">The routing key specified by the feed</param>
        /// <param name="messageTypeName">The type name of the received message</param>
        /// <param name="sportId">If the method returned true the sportId; Otherwise undefined</param>
        /// <returns>True if sportId could be retrieved from <code>routingKey</code>; Otherwise false</returns>
        public bool TryGetSportId(string routingKey, string messageTypeName, out URN sportId)
        {
            sportId = null;
            try
            {
                sportId = GetSportId(routingKey, messageTypeName);
            }
            catch (FormatException)
            {
                return false;
            }
            return true;
        }
    }
}
