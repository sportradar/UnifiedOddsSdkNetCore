/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Castle.Core.Internal;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Entities.REST.Market;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Common.Internal
{
    /// <summary>
    /// Class provides information about current executing assembly
    /// </summary>
    internal static class SdkInfo
    {
        /// <summary>
        /// Internal sdk logger to be used within base classes - so no need in every class
        /// </summary>
        public static readonly ILogger ExecutionLog = SdkLoggerFactory.GetLoggerForExecution(typeof(SdkInfo));

        /// <summary>
        /// The unknown producer identifier
        /// </summary>
        public const int UnknownProducerId = 99;
        /// <summary>
        /// The specifiers delimiter
        /// </summary>
        public const string SpecifiersDelimiter = "|";
        /// <summary>
        /// The market groups delimiter
        /// </summary>
        public const string MarketGroupsDelimiter = "|";
        /// <summary>
        /// The market mapping products delimiter
        /// </summary>
        public const string MarketMappingProductsDelimiter = "|";
        /// <summary>
        /// The variant description name
        /// </summary>
        public const string VariantDescriptionName = "variant";
        /// <summary>
        /// The outcometext variant value
        /// </summary>
        public const string OutcomeTextVariantValue = "pre:outcometext";
        /// <summary>
        /// The free text variant value
        /// </summary>
        public const string FreeTextVariantValue = "free_text";
        /// <summary>
        /// The player props market group
        /// </summary>
        public const string PlayerPropsMarketGroup = "player_props";
        /// <summary>
        /// The flex score market attribute name
        /// </summary>
        public const string FlexScoreMarketAttributeName = "is_flex_score";
        /// <summary>
        /// The competitors market group
        /// </summary>
        public const string CompetitorsMarketOutcomeType = "competitors";
        /// <summary>
        /// The competitor market group
        /// </summary>
        public const string CompetitorMarketOutcomeType = "competitor";
        /// <summary>
        /// The player market group
        /// </summary>
        public const string PlayerMarketOutcomeType = "player";
        /// <summary>
        /// The minimum inactivity seconds
        /// </summary>
        public const int MinInactivitySeconds = 20;
        /// <summary>
        /// The maximum inactivity seconds
        /// </summary>
        public const int MaxInactivitySeconds = 180;
        /// <summary>
        /// The minimum recovery execution in seconds
        /// </summary>
        public const int MinRecoveryExecutionInSeconds = 600;
        /// <summary>
        /// The maximum recovery execution in seconds
        /// </summary>
        public const int MaxRecoveryExecutionInSeconds = 3600;
        /// <summary>
        /// The minimal interval between recovery requests initiated by alive messages (seconds)
        /// </summary>
        public const int MinIntervalBetweenRecoveryRequests = 20;
        /// <summary>
        /// The maximum interval between recovery requests initiated by alive messages (seconds)
        /// </summary>
        public const int MaxIntervalBetweenRecoveryRequests = 180;
        /// <summary>
        /// The default interval between recovery requests initiated by alive messages (seconds)
        /// </summary>
        public const int DefaultIntervalBetweenRecoveryRequests = 30;
        /// <summary>
        /// Defines the beginning of the outcome id for player outcomes
        /// </summary>
        public const string PlayerProfileMarketPrefix = "sr:player:";
        /// <summary>
        /// Defines the beginning of the outcome id for competitor outcomes
        /// </summary>
        public const string CompetitorProfileMarketPrefix = "sr:competitor";
        /// <summary>
        /// Defines the type of the player URI
        /// </summary>
        public const string PlayerProfileIdentifier = "player";
        /// <summary>
        /// The separator used when the outcome id is a composite of multiple ids - sr:player:1,sr:player:2
        /// </summary>
        public const string NameProviderCompositeIdSeparator = ",";
        /// <summary>
        /// The iso8601 24h short format
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public const string ISO8601_24H_Format = "yyyy-MM-dd’T’HH:mm:ss";
        /// <summary>
        /// The iso8601 24h full format
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public const string ISO8601_24H_FullFormat = "yyyy-MM-dd’T’HH:mm:ssXXX";
        /// <summary>
        /// The market description minimum fetch interval in seconds
        /// </summary>
        public const int MarketDescriptionMinFetchInterval = 30;
        /// <summary>
        /// The minimum HTTP timeout
        /// </summary>
        public const int DefaultHttpClientTimeout = 30;
        /// <summary>
        /// The minimum HTTP timeout
        /// </summary>
        public const int MinHttpClientTimeout = 10;
        /// <summary>
        /// The maximum HTTP timeout
        /// </summary>
        public const int MaxHttpClientTimeout = 100;
        /// <summary>
        /// The rest connection failure limit
        /// </summary>
        public const int RestConnectionFailureLimit = 5;
        /// <summary>
        /// The rest connection failure timeout in sec
        /// </summary>
        public const int RestConnectionFailureTimeoutInSec = 15;
        /// <summary>
        /// The soccer sport urns
        /// </summary>
        public static readonly IReadOnlyCollection<URN> SoccerSportUrns = new[] {URN.Parse("sr:sport:1"), URN.Parse("sr:sport:137")};

        /// <summary>
        /// Gets the assembly version number
        /// </summary>
        /// <returns>System.String</returns>
        public static string GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        /// <summary>
        /// Logs the SDK version at the beginning of opening feed
        /// </summary>
        /// <param name="log">The log used for logging</param>
        public static void LogSdkVersion(ILogger log)
        {
            if (log != null)
            {
                log.LogInformation("=============================");
                log.LogInformation($"    UF SDK .NET ({GetVersion()}) ");
                log.LogInformation("=============================");
            }
        }

        /// <summary>
        /// Convert long epoch time to DateTime
        /// </summary>
        /// <param name="epochTime">The unix time</param>
        /// <returns>DateTime</returns>
        public static DateTime FromEpochTime(long epochTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(epochTime).ToLocalTime();
        }

        /// <summary>
        /// Convert long epoch time to DateTime
        /// </summary>
        /// <param name="epochTime">The unix time</param>
        /// <returns>DateTime</returns>
        public static DateTime? TryFromEpochTime(long epochTime)
        {
            try
            {
                var dateTime = FromEpochTime(epochTime);
                return dateTime;
            }
            catch (Exception)
            {
                //ignored
            }
            return null;
        }

        /// <summary>
        /// Convert DateTime to the epoch time (in seconds)
        /// </summary>
        /// <param name="date">The date</param>
        /// <returns>System.Int64</returns>
        public static long ToEpochTime(DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date.ToUniversalTime() - epoch).TotalMilliseconds);
        }

        /// <summary>
        /// Get decimal value as string with sign
        /// </summary>
        /// <param name="input">The decimal input</param>
        /// <returns>The string representation of the value with the sign (except zero)</returns>
        /// <example>
        /// 0 -&gt; 0
        /// -1 -&gt; -1
        /// 1 -&gt; +1
        /// -1.25 -&gt; -1.25
        /// 1.25 -&gt; +1.25
        /// </example>
        public static string DecimalToStringWithSign(decimal input)
        {
            string result;
            if (input > 0)
            {
                result = $"+{input.ToString(CultureInfo.InvariantCulture)}";
            }
            else if (input < 0)
            {
                result = $"{input.ToString(CultureInfo.InvariantCulture)}";
            }
            else
            {
                result = "0";
            }
            return result;
        }

        /// <summary>
        /// Parses the date
        /// </summary>
        /// <param name="input">The string representation of the date to be parsed</param>
        /// <returns>System.Nullable&lt;DateTime&gt;</returns>
        public static DateTime? ParseDate(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            if (DateTime.TryParse(input, out var date))
            {
                return date;
            }

            if (DateTime.TryParseExact(input, ISO8601_24H_FullFormat, new DateTimeFormatInfo(), DateTimeStyles.None, out date))
            {
                return date;
            }

            if (DateTime.TryParseExact(input, ISO8601_24H_Format, new DateTimeFormatInfo(), DateTimeStyles.None, out date))
            {
                return date;
            }

            return date;
        }

        /// <summary>
        /// Gets the name of the abbreviation from
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="length">The length of the abbreviation</param>
        /// <returns>Get abbreviation from name</returns>
        public static string GetAbbreviationFromName(string name, int length = 3)
        {
            if (length < 1)
            {
                length = int.MaxValue;
            }
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            return name.Length > length ? name.Substring(0, length).ToUpper() : name.ToUpper();
        }

        /// <summary>
        /// Combines the date and time
        /// </summary>
        /// <param name="date">The date</param>
        /// <param name="time">The time</param>
        /// <returns>Returns combined date and time</returns>
        public static DateTime CombineDateAndTime(DateTime date, DateTime time)
        {
            return date.AddHours(time.ToUniversalTime().Hour).AddMinutes(time.ToUniversalTime().Minute).AddSeconds(time.Second);
        }

        /// <summary>
        /// Dictionaries to string of key-value pairs
        /// </summary>
        /// <param name="specifiers">The specifiers.</param>
        /// <returns>System.String.</returns>
        public static string DictionaryToString(IDictionary<string, string> specifiers)
        {
            if (specifiers == null || !specifiers.Any())
            {
                return string.Empty;
            }
            var tmp = specifiers.Aggregate(string.Empty, (current, pair) => current + $"{pair.Key}={pair.Value}|");
            return tmp.Remove(tmp.Length - 1);
        }

        /// <summary>
        /// List of <see cref="ISpecifier"/> to string
        /// </summary>
        /// <param name="specifiers">The specifiers.</param>
        /// <returns>System.String.</returns>
        public static string SpecifiersToString(IEnumerable<ISpecifier> specifiers)
        {
            if (specifiers == null)
            {
                return string.Empty;
            }
            var tmp = string.Join("|", specifiers);
            return tmp;
        }

        /// <summary>
        /// Get the list of specifier names
        /// </summary>
        /// <param name="specifiers">The specifiers.</param>
        /// <returns>System.String.</returns>
        public static string SpecifiersKeysToString(IEnumerable<ISpecifier> specifiers)
        {
            if (specifiers == null)
            {
                return string.Empty;
            }

            var tmp = string.Join(",", specifiers.Select(s => s.Name));
            return tmp;
        }

        /// <summary>
        /// Gets the fixed length unique identifier
        /// </summary>
        /// <param name="length">The length of the returned string</param>
        /// <param name="containDash">if set to <c>true</c> [contain dash].</param>
        public static string GetGuid(int length, bool containDash = false)
        {
            var g = Guid.NewGuid().ToString();

            if (!containDash)
            {
                g = g.Replace("-", string.Empty);
            }

            if (length > 0 && length < g.Length)
            {
                g = g.Substring(0, length);
            }

            return g;
        }

        /// <summary>
        /// Clear sensitive data (removes part of it)
        /// </summary>
        /// <param name="input">Data to clear</param>
        /// <returns>Cleared input string</returns>
        public static string ClearSensitiveData(string input)
        {
            return !string.IsNullOrEmpty(input) && input.Length > 3
                       ? input.Substring(0, 3) + "***" + input.Substring(input.Length - 3)
                       : input;
        }

        /// <summary>
        /// Clear sensitive data (removes part of it) from then input text
        /// </summary>
        /// <param name="input">Data to clear</param>
        /// <param name="sensitiveData">Search for this data and replace with cleared input string</param>
        /// <returns>Cleared input string</returns>
        public static string ClearSensitiveData(string input, string sensitiveData)
        {
            if (input.IsNullOrEmpty())
            {
                return input;
            }

            var clearedData = ClearSensitiveData(sensitiveData);

            return input.Replace(sensitiveData, clearedData);
        }
    }
}
