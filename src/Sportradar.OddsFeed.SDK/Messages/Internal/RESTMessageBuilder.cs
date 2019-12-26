/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Messages.Internal
{
    /// <summary>
    /// A helper class providing an easier way of constructing instances which have only default constructor
    /// </summary>
    public static class RestMessageBuilder
    {
        /// <summary>
        /// Builds the coverage record
        /// </summary>
        /// <param name="maxCoverageLevel">The maximum coverage level</param>
        /// <param name="minCoverageLevel">The minimum coverage level</param>
        /// <param name="maxCovered">The maximum covered</param>
        /// <param name="played">The played</param>
        /// <param name="scheduled">The scheduled</param>
        /// <param name="seasonId">The season identifier</param>
        /// <returns>seasonCoverageInfo</returns>
        public static seasonCoverageInfo BuildCoverageRecord(string maxCoverageLevel, string minCoverageLevel, int? maxCovered, int played, int scheduled, string seasonId)
        {
            var record = new seasonCoverageInfo
            {
                max_coverage_level = maxCoverageLevel,
                min_coverage_level = minCoverageLevel,
                played = played,
                scheduled = scheduled,
                season_id = seasonId
            };

            if (maxCovered != null)
            {
                record.max_covered = maxCovered.Value;
                record.max_coveredSpecified = true;
            }

            return record;
        }

        /// <summary>
        /// Builds the season extended record
        /// </summary>
        /// <param name="id">The identifier</param>
        /// <param name="name">The name</param>
        /// <param name="startDate">The start date</param>
        /// <param name="endDate">The end date</param>
        /// <param name="year">The year</param>
        /// <returns>seasonExtended</returns>
        public static seasonExtended BuildSeasonExtendedRecord(string id, string name, DateTime startDate, DateTime endDate, string year)
        {
            return new seasonExtended
            {
                id = id,
                name = name,
                start_date = startDate,
                end_date = endDate,
                year = year
            };
        }

        /// <summary>
        /// Builds the bookmaker details
        /// </summary>
        /// <param name="id">The identifier</param>
        /// <param name="expiresAt">The expires at</param>
        /// <param name="responseCode">The response code</param>
        /// <param name="virtualHost">The virtual host</param>
        /// <returns>bookmaker_details</returns>
        public static bookmaker_details BuildBookmakerDetails(int? id, DateTime? expiresAt, response_code? responseCode, string virtualHost)
        {
            var record = new bookmaker_details
            {
                bookmaker_id = id ?? 0,
                bookmaker_idSpecified = id != null,
                expire_atSpecified = expiresAt != null,
                response_codeSpecified = responseCode != null,
                virtual_host = virtualHost
            };

            if (responseCode != null)
            {
                record.response_code = responseCode.Value;
            }
            if (expiresAt != null)
            {
                record.expire_at = expiresAt.Value;
            }

            return record;
        }
    }
}
