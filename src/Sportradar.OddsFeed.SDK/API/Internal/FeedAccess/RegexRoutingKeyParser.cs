// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Text.RegularExpressions;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;

namespace Sportradar.OddsFeed.SDK.Api.Internal.FeedAccess
{
    /// <summary>
    /// A <see cref="IRoutingKeyParser"/> implementation which uses regex.
    /// </summary>
    internal class RegexRoutingKeyParser : IRoutingKeyParser
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
        /// Gets a <see cref="Urn"/> representing the sportId by parsing the provided <c>routingKey</c>
        /// </summary>
        /// <param name="routingKey">The routing key specified by the feed</param>
        /// <param name="messageTypeName">The type name of the received message</param>
        /// <returns>The sportId obtained by parsing the provided <c>routingKey</c></returns>
        public Urn GetSportId(string routingKey, string messageTypeName)
        {
            if (messageTypeName.Equals("alive", StringComparison.InvariantCultureIgnoreCase) ||
                messageTypeName.Equals("snapshot_complete", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            var match = Regex.Match(routingKey, string.Format(RegexPatternFormat, messageTypeName), RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
            if (!match.Success)
            {
                throw new FormatException($"The format of string {routingKey} is not correct");
            }

            return Urn.Parse($"{SportIdPrefix + match.Groups[GroupName].Value}");
        }

        /// <summary>
        /// Tries to get a <see cref="Urn"/> representing of the sportId by parsing the provided <c>routingKey</c>
        /// </summary>
        /// <param name="routingKey">The routing key specified by the feed</param>
        /// <param name="messageTypeName">The type name of the received message</param>
        /// <param name="sportId">If the method returned true the sportId; Otherwise undefined</param>
        /// <returns>True if sportId could be retrieved from <c>routingKey</c>; Otherwise false</returns>
        public bool TryGetSportId(string routingKey, string messageTypeName, out Urn sportId)
        {
            sportId = null;
            try
            {
                if (routingKey.IsNullOrEmpty() ||
                    messageTypeName.Equals("alive", StringComparison.InvariantCultureIgnoreCase) ||
                    messageTypeName.Equals("snapshot_complete", StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }

                sportId = GetSportId(routingKey, messageTypeName);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
