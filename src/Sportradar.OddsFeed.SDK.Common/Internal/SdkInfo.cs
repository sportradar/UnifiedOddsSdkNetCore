/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Globalization;
using System.Reflection;
using Common.Logging;

namespace Sportradar.OddsFeed.SDK.Common.Internal
{
    /// <summary>
    /// Class provides information about current executing assembly
    /// </summary>
    public class SdkInfo
    {
        /// <summary>
        /// The production host
        /// </summary>
        public const string ProductionHost = "mq.betradar.com";
        /// <summary>
        /// The production API host
        /// </summary>
        public const string ProductionApiHost = "api.betradar.com";
        /// <summary>
        /// The integration host
        /// </summary>
        public const string IntegrationHost = "stgmq.betradar.com";
        /// <summary>
        /// The integration API host
        /// </summary>
        public const string IntegrationApiHost = "stgapi.betradar.com";

        /// <summary>
        /// The replay host
        /// </summary>
        public const string ReplayHost = "replaymq.betradar.com";
        /// <summary>
        /// The replay API host
        /// </summary>
        public const string ReplayApiHost = ProductionApiHost;

        /// <summary>
        /// The default host port
        /// </summary>
        public const int DefaultHostPort = 5671;

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
        public const string OutcometextVariantValue = "pre:outcometext";
        /// <summary>
        /// The outcometext variant value
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
        /// Defines the beginning of the outcome id for player outcomes
        /// </summary>
        public const string PlayerProfileMarketPrefix = "sr:player:";
        /// <summary>
        /// Defines the beginning of the outcome id for competitor outcomes
        /// </summary>
        public const string CompetitorProfileMarketPrefix = "sr:competitor";
        /// <summary>
        /// Defines the type of the simple team URI
        /// </summary>
        public const string SimpleTeamIdentifier = "simpleteam";
        /// <summary>
        /// Defines the type of the player URI
        /// </summary>
        public const string PlayerProfileIdentifier = "player";
        /// <summary>
        /// The separator used when the outcome id is a composite of multiple ids - sr:player:1,sr:player:2
        /// </summary>
        public const string NameProviderCompositeIdSeparator = ",";
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
        public static void LogSdkVersion(ILog log)
        {
            if (log != null)
            {
                log.Info("=============================");
                log.Info($"    UF SDK .NET ({GetVersion()}) ");
                log.Info("=============================");
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
                result = $"+{input}";
            }
            else if (input < 0)
            {
                result = $"{input}";
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

            DateTime date;

            if (DateTime.TryParse(input, out date))
            {
                return date;
            }

            DateTime.TryParseExact(input, ISO8601_24H_FullFormat, new DateTimeFormatInfo(), DateTimeStyles.None, out date);

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
    }
}
