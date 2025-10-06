// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest.Market;

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
        /// The type of the sdk
        /// </summary>
        public const string SdkType = "NETStd";
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
        /// Defines the type of Urn part of the outcome id for player outcomes
        /// </summary>
        public const string PlayerProfileGroupIdentifierForMarket = ":player:";
        /// <summary>
        /// Defines the type of Urn part of the outcome id for competitor outcomes
        /// </summary>
        public const string CompetitorProfileGroupIdentifierForMarket = ":competitor:";
        /// <summary>
        /// Defines the type of the player URI
        /// </summary>
        public const string PlayerProfileIdentifier = "player";
        /// <summary>
        /// The separator used when the outcome id is a composite of multiple ids - sr:player:1,sr:player:2
        /// </summary>
        public const string NameProviderCompositeIdSeparator = ",";
        /// <summary>
        /// The Iso8601 24h short format
        /// </summary>
        public const string Iso860124HFormat = "yyyy-MM-dd’T’HH:mm:ss";
        /// <summary>
        /// The Iso8601 24h full format
        /// </summary>
        public const string Iso860124HFullFormat = "yyyy-MM-dd’T’HH:mm:ssXXX";
        /// <summary>
        /// The date when it was created
        /// </summary>
        public static readonly DateTime Created = DateTime.Now;
        /// <summary>
        /// The regex pattern to extract error message from failed API requests
        /// </summary>
        public const string ApiResponseErrorPattern = @"<errors>([a-zA-Z0-9 -_\:.\/'{}]*)<\/errors>";
        /// <summary>
        /// The regex pattern to extract response message from failed API requests
        /// </summary>
        public const string ApiResponseMessagePattern = @"<message>([a-zA-Z0-9 -_\:.]*)<\/message>";
        /// <summary>
        /// The semaphore pool size
        /// </summary>
        public const int SemaphorePoolSize = 600;
        /// <summary>
        /// The regex to check valid market id for within market description market mapping (valid: "5", "5:123")
        /// </summary>
        public const string MarketMappingMarketIdValidPattern = @"^-?\d+(:-?\d+)?$";
        /// <summary>
        /// The regex to check valid producer_ids attribute for within market description market mapping (valid: "5", "1|3")
        /// </summary>
        public const string MarketMappingProducerIdsValidPattern = @"^(\d+(\|\d+)*)?$";

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
                log.LogInformation("    UF SDK .NET ({UofSdkVersion}) ", GetVersion());
                log.LogInformation("=============================");
            }
        }

        /// <summary>
        /// Convert long epoch time to DateTime
        /// </summary>
        /// <param name="epochTime">The Unix time</param>
        /// <returns>DateTime</returns>
        public static DateTime FromEpochTime(long epochTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(epochTime).ToLocalTime();
        }

        /// <summary>
        /// Convert long epoch time to DateTime
        /// </summary>
        /// <param name="epochTime">The Unix time</param>
        /// <returns>DateTime</returns>
        public static DateTime? TryFromEpochTime(long epochTime)
        {
            try
            {
                var dateTime = FromEpochTime(epochTime);
                return dateTime;
            }
            catch
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

            if (DateTime.TryParseExact(input, Iso860124HFullFormat, new DateTimeFormatInfo(), DateTimeStyles.None, out date))
            {
                return date;
            }

            if (DateTime.TryParseExact(input, Iso860124HFormat, new DateTimeFormatInfo(), DateTimeStyles.None, out date))
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
            return name.Length > length ? name.Substring(0, length).ToUpperInvariant() : name.ToUpperInvariant();
        }

        /// <summary>
        /// Combines the date and time
        /// </summary>
        /// <param name="date">The date</param>
        /// <param name="time">The time</param>
        /// <returns>Returns combined date and time</returns>
        public static DateTime CombineDateAndTime(DateTime date, DateTime time)
        {
            return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, time.Millisecond, DateTimeKind.Utc);
        }

        /// <summary>
        /// Specifiers dictionary to string of key-value pairs
        /// </summary>
        /// <param name="specifiers">The specifiers.</param>
        /// <returns>System.String.</returns>
        public static string SpecifiersDictionaryToString(IDictionary<string, string> specifiers)
        {
            if (specifiers.IsNullOrEmpty())
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
        /// List of specifiers as string
        /// </summary>
        /// <param name="specifiers">The specifiers.</param>
        /// <returns>Dictionary of specifiers</returns>
        public static IDictionary<string, string> SpecifiersStringToDictionary(string specifiers)
        {
            var result = new Dictionary<string, string>();
            if (specifiers.IsNullOrEmpty())
            {
                return result;
            }
            var specs = specifiers.Split('|');
            foreach (var spec in specs)
            {
                var specKeyValue = spec.Split('=');
                result.Add(specKeyValue[0], specKeyValue[1]);
            }
            return result;
        }

        /// <summary>
        /// Dictionary to comma-delimited string of key-value pairs
        /// </summary>
        /// <param name="values">The values</param>
        /// <returns>The comma-delimited string</returns>
        public static string DictionaryToString(IDictionary<string, string> values)
        {
            return values.IsNullOrEmpty()
                       ? string.Empty
                       : string.Join(", ", values.Select(keyValuePair => keyValuePair.Key + "=" + keyValuePair.Value));
        }

        /// <summary>
        /// Gets the fixed length unique identifier
        /// </summary>
        /// <param name="length">The length of the returned string (min 3)</param>
        /// <param name="containDash">if set to <c>true</c> [contain dash].</param>
        public static string GetGuid(int length, bool containDash = false)
        {
            if (length < 3)
            {
                length = 3;
            }
            else if (length > 20)
            {
                length = 20;
            }

            var g = Guid.NewGuid().ToString();

            if (!containDash)
            {
                g = g.Replace("-", string.Empty);
            }

            if (length < g.Length)
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

        /// <summary>
        /// Check if object is numeric value
        /// </summary>
        /// <param name="expression">The object to check</param>
        /// <returns>Returns <c>true</c> if object represents numeric value</returns>
        public static bool IsNumeric(object expression)
        {
            var isNum = double.TryParse(Convert.ToString(expression), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out _);
            return isNum;
        }

        /// <summary>
        /// Get new value based on input and variable percent (up or down)
        /// </summary>
        /// <param name="baseValue">The base value</param>
        /// <param name="variablePercent">The max percent to deviate from base value</param>
        /// <returns>The new value within min-max</returns>
        public static TimeSpan GetVariableNumber(TimeSpan baseValue, int variablePercent = 5)
        {
            var newValue = GetVariableNumber(Convert.ToInt32(baseValue.TotalMilliseconds), variablePercent);
            return TimeSpan.FromMilliseconds(newValue);
        }

        /// <summary>
        /// Get new value based on input and variable percent (up or down)
        /// </summary>
        /// <param name="baseValue">The base value</param>
        /// <param name="variablePercent">The max percent to deviate from base value</param>
        /// <returns>The new value within min-max</returns>
        public static int GetVariableNumber(int baseValue, int variablePercent = 5)
        {
            if (baseValue < 1 || variablePercent == 0)
            {
                return baseValue;
            }

            if (variablePercent < 0 || variablePercent > 100)
            {
                variablePercent = 10;
            }

            var start = (100 - variablePercent) / (double)100 * baseValue;
            var end = (100 + variablePercent) / (double)100 * baseValue;

            return GetRandom((int)start, (int)end);
        }

        /// <summary>
        /// Get new value based on input and variable percent (up only)
        /// </summary>
        /// <param name="baseValue">The base value</param>
        /// <param name="variablePercent">The max percent to deviate from base value</param>
        /// <returns>The new value within base-max</returns>
        public static TimeSpan AddVariableNumber(TimeSpan baseValue, int variablePercent = 5)
        {
            var newValue = AddVariableNumber(Convert.ToInt32(baseValue.TotalMilliseconds), variablePercent);
            return TimeSpan.FromMilliseconds(newValue);
        }

        /// <summary>
        /// Get new value based on input and variable percent (up only)
        /// </summary>
        /// <param name="baseValue">The base value</param>
        /// <param name="variablePercent">The max percent to deviate from base value</param>
        /// <returns>The new value within base-max</returns>
        public static int AddVariableNumber(int baseValue, int variablePercent = 5)
        {
            if (baseValue < 1 || variablePercent == 0)
            {
                return baseValue;
            }

            if (variablePercent < 0 || variablePercent > 100)
            {
                variablePercent = 10;
            }

            var end = (100 + variablePercent) / (double)100 * baseValue;

            return GetRandom(baseValue, (int)end);
        }

        /// <summary>
        /// Get the feed message age; how behind is at the time of consuming by the sdk
        /// </summary>
        /// <param name="generatedAt">The generatedAt timestamp from the message</param>
        /// <param name="receivedAt">The timestamp when message consumed by the sdk</param>
        /// <returns>The difference in ms</returns>
        public static long GetMessageAge(long generatedAt, long receivedAt)
        {
            var age = receivedAt - generatedAt;
            if (age < 0)
            {
                age = 0;
            }
            return age;
        }

        /// <summary>
        /// Get the feed message age; how behind is at the time of consuming by the sdk (calculated via DateTime)
        /// </summary>
        /// <param name="generatedAt">The generatedAt timestamp from the message</param>
        /// <param name="receivedAt">The timestamp when message consumed by the sdk</param>
        /// <returns>The difference in ms</returns>
        public static long GetMessageAge2(long generatedAt, long receivedAt)
        {
            var age = FromEpochTime(receivedAt) - FromEpochTime(generatedAt);
            if (receivedAt < generatedAt)
            {
                age = TimeSpan.Zero;
            }
            return (long)age.TotalMilliseconds;
        }

        public static int GetRandom(int maxValue = int.MaxValue)
        {
            return GetRandom(0, maxValue);
        }

        public static int GetRandom(int minValue, int maxValue)
        {
            if (minValue >= maxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(minValue), $"MinValue [{minValue}] not valid. Must be less then MaXValue [{maxValue}].");
            }

            using (var generator = RandomNumberGenerator.Create())
            {
                var bytes = new byte[4];
                var range = maxValue - minValue;
                generator.GetBytes(bytes);
                var generatedNumber = BitConverter.ToInt32(bytes, 0);
                return Math.Abs(generatedNumber % range) + minValue;
            }
        }

        public static int GetMidValue(int initialValue, int minValue = 0, int maxValue = int.MaxValue)
        {
            if (initialValue < minValue)
            {
                return minValue;
            }
            if (initialValue > maxValue)
            {
                return maxValue;
            }
            return initialValue;
        }

        public static string ExtractHttpResponseMessage(HttpContent responseContent)
        {
            if (responseContent == null)
            {
                return string.Empty;
            }

            var response = responseContent.ReadAsStringAsync().GetAwaiter().GetResult();
            return ExtractHttpResponseMessage(response);
        }

        public static string ExtractHttpResponseMessage(string responseContent)
        {
            if (responseContent.IsNullOrEmpty())
            {
                return string.Empty;
            }
            var errorMatch = Regex.Match(responseContent, ApiResponseErrorPattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
            var messageMatch = Regex.Match(responseContent, ApiResponseMessagePattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
            if (messageMatch.Success)
            {
                return errorMatch.Success ? $"{messageMatch.Groups[1].Value} (detail: {errorMatch.Groups[1].Value})" : messageMatch.Groups[1].Value;
            }
            return responseContent;
        }

        /// <summary>
        /// Calculates the length in bytes of an object and returns the size
        /// </summary>
        /// <param name="serializableObject">Object to get size</param>
        /// <returns>The size in bytes</returns>
        public static int GetObjectSize(object serializableObject)
        {
            if (serializableObject == null)
            {
                return 0;
            }
            if (!serializableObject.GetType().IsSerializable)
            {
                return 1;
            }
            using (var ms = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(serializableObject.GetType());
                serializer.WriteObject(ms, serializableObject);
                var jsonString = Encoding.UTF8.GetString(ms.ToArray());

                var bytes = Encoding.UTF8.GetBytes(jsonString);
                return bytes.Length;
            }
        }

        /// <summary>
        /// Calculates the length in bytes of an object and returns the size
        /// </summary>
        /// <param name="serializableObject">Object to get size</param>
        /// <returns>The size in bytes</returns>
        public static int TryGetObjectSize(object serializableObject)
        {
            try
            {
                return GetObjectSize(serializableObject);
            }
            catch
            {
                return serializableObject == null ? 0 : 1;
            }
        }

        /// <summary>
        /// Convert list of CultureInfo to comma delimited string
        /// </summary>
        /// <param name="cultures"></param>
        /// <returns></returns>
        public static string ConvertCultures(ICollection<CultureInfo> cultures)
        {
            return cultures.IsNullOrEmpty()
                       ? string.Empty
                       : string.Join(",", cultures.Select(s => s.TwoLetterISOLanguageName));
        }

        /// <summary>
        /// Get the DateTime.UtcNow formatted as string
        /// </summary>
        /// <returns>The DateTime.UtcNow formatted as string</returns>
        public static string UtcNowString()
        {
            return DateTime.UtcNow.ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture);
        }

        public static IReadOnlyDictionary<CultureInfo, string> GetOrCreateReadOnlyNames(IDictionary<CultureInfo, string> availableNames, IReadOnlyList<CultureInfo> wantedCultures)
        {
            return GetOrCreateReadOnlyNames(availableNames, wantedCultures as IReadOnlyCollection<CultureInfo>);
        }

        public static IReadOnlyDictionary<CultureInfo, string> GetOrCreateReadOnlyNames(IDictionary<CultureInfo, string> availableNames, IReadOnlyCollection<CultureInfo> wantedCultures)
        {
            if (wantedCultures.IsNullOrEmpty())
            {
                return new Dictionary<CultureInfo, string>();
            }

            if (availableNames.IsNullOrEmpty())
            {
                return wantedCultures.ToDictionary(s => s, s => string.Empty);
            }

            return wantedCultures.ToDictionary(s => s, s => availableNames.TryGetValue(s, out var name) ? name : string.Empty);
        }

        public static string FriendlyTimeSpanTextInHours(TimeSpan timeSpan)
        {
            var result = new StringBuilder();
            //if (timeSpan.Days > 0)
            //{
            //    int weeks = timeSpan.Days / 7;
            //    int days = timeSpan.Days % 7;

            //    if (weeks > 0)
            //        result.Append($"{weeks}w ");
            //    if (days > 0)
            //        result.Append($"{days}d ");
            //}

            if (timeSpan.TotalHours <= -1 || timeSpan.TotalHours >= 1)
            {
                result.Append($"{Convert.ToInt32(timeSpan.TotalHours)}h");
            }

            if (timeSpan.Minutes != 0)
            {
                if (result.Length > 0)
                {
                    result.Append(" ");
                }
                result.Append($"{timeSpan.Minutes}min");
            }

            if (timeSpan.Seconds != 0)
            {
                if (result.Length > 0)
                {
                    result.Append(" ");
                }
                result.Append($"{timeSpan.Seconds}sec");
            }
            if (result.Length == 0)
            {
                result.Append("0sec");
            }

            return result.ToString().Trim();
        }

        public static bool IsMarketMappingMarketIdValid(string marketId)
        {
            return Regex.IsMatch(marketId, MarketMappingMarketIdValidPattern, RegexOptions.None, TimeSpan.FromMilliseconds(100));
        }

        public static bool IsMarketMappingProducerIdsValid(string producerIds)
        {
            if (producerIds.IsNullOrEmpty())
            {
                return true;
            }

            if (Regex.IsMatch(producerIds, MarketMappingProducerIdsValidPattern, RegexOptions.None, TimeSpan.FromMilliseconds(100)))
            {
                var producerIdList = producerIds.Split('|');
                return producerIdList.All(a => int.Parse(a) > 0);
            }

            return false;
        }
    }
}
