using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.API.Test
{
    public static class ConfigurationSectionExtensions
    {
        internal static IOddsFeedConfigurationSection ToSection(this string xmlConfig)
        {
            using var stringReader = new StringReader(xmlConfig);
            using var xmlReader = XmlReader.Create(stringReader);
            var section = new OddsFeedConfigurationSection();
            var deserializer = section.GetType().GetMethod("DeserializeSection",
                                                           BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, null, new[] {typeof(XmlReader)},
                                                           null);
            deserializer?.Invoke(section, new object[] {xmlReader});
            return section;
        }

        public static IOddsFeedConfiguration ToSdkConfiguration(this string xmlConfig)
        {
            using var stringReader = new StringReader(xmlConfig);
            using var xmlReader = XmlReader.Create(stringReader);
            return xmlReader.ToSdkConfiguration();
        }

        private static IOddsFeedConfiguration ToSdkConfiguration(this XmlReader reader)
        {
            var section = new OddsFeedConfigurationSection();
            var deserializer = section.GetType().GetMethod("DeserializeSection",
                                                          BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, null, new[] {typeof(XmlReader)},
                                                          null);
            deserializer?.Invoke(section, new object[] {reader});

            var sdkEnvironment = SdkEnvironment.Integration;
            if (section.UfEnvironment != null)
            {
                sdkEnvironment = section.UfEnvironment.Value;
            }
            else if (!section.UseIntegrationEnvironment)
            {
                sdkEnvironment = SdkEnvironment.Production;
            }

            var supportedLanguages = new List<CultureInfo>();
            if (!string.IsNullOrEmpty(section.SupportedLanguages))
            {
                var langCodes = section.SupportedLanguages.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                supportedLanguages = langCodes.Select(langCode => new CultureInfo(langCode.Trim())).ToList();
            }

            var defaultLanguage = supportedLanguages.Any() ? supportedLanguages.First() : null;
            if (!string.IsNullOrEmpty(section.DefaultLanguage))
            {
                defaultLanguage = new CultureInfo(section.DefaultLanguage);
                if (!supportedLanguages.Contains(defaultLanguage))
                {
                    supportedLanguages.Insert(0, defaultLanguage);
                }
            }

            var disabledProducers = new List<int>();
            if (!string.IsNullOrEmpty(section.DisabledProducers))
            {
                var producerIds = section.DisabledProducers.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                disabledProducers = producerIds.Select(producerId => int.Parse(producerId.Trim())).ToList();
            }

            return new OddsFeedConfiguration(section.AccessToken,
                                             sdkEnvironment,
                                             defaultLanguage,
                                             supportedLanguages,
                                             EnvironmentManager.GetMqHost(sdkEnvironment),
                                             section.VirtualHost,
                                             section.Port,
                                             section.Username,
                                             section.Password,
                                             EnvironmentManager.GetApiHost(sdkEnvironment),
                                             section.UseSSL,
                                             section.UseApiSSL,
                                             section.InactivitySeconds,
                                             section.MaxRecoveryTime,
                                             section.MinIntervalBetweenRecoveryRequests,
                                             section.NodeId,
                                             disabledProducers,
                                             section.ExceptionHandlingStrategy,
                                             section.AdjustAfterAge,
                                             section.HttpClientTimeout,
                                             section.RecoveryHttpClientTimeout,
                                             section);
        }
    }
}
