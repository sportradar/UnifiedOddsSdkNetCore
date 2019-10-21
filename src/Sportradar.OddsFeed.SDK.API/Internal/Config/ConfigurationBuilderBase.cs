/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using Dawn;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common;

namespace Sportradar.OddsFeed.SDK.API.Internal.Config
{
    /// <summary>
    /// Represents a base class for configuration builders
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal abstract class ConfigurationBuilderBase<T> : IConfigurationBuilderBase<T> where T : class
    {
        /// <summary>
        /// The <see cref="IConfigurationSectionProvider"/> containing data read from the config file
        /// </summary>
        internal readonly IConfigurationSectionProvider SectionProvider;

        /// <summary>
        /// The access token used for authentication
        /// </summary>
        internal readonly string AccessToken;

        /// <summary>
        /// A list of default cultures / languages
        /// </summary>
        protected readonly List<CultureInfo> SupportedLocales = new List<CultureInfo>();

        /// <summary>
        /// The default locale
        /// </summary>
        protected CultureInfo DefaultLocale;

        /// <summary>
        /// The list of disabled producers
        /// </summary>
        protected List<int> DisabledProducers;

        /// <summary>
        /// The node identifier
        /// </summary>
        protected ExceptionHandlingStrategy ExceptionHandlingStrategy;

        /// <summary>
        /// The node identifier
        /// </summary>
        protected int NodeId;

        internal IOddsFeedConfigurationSection Section { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationBuilderBase{T}"/> class
        /// </summary>
        /// <param name="accessToken">An access token used to authenticate with the feed</param>
        /// <param name="sectionProvider">A <see cref="IConfigurationSectionProvider"/> used to access <see cref="IOddsFeedConfigurationSection"/></param>
        internal ConfigurationBuilderBase(string accessToken, IConfigurationSectionProvider sectionProvider)
        {
            Guard.Argument(accessToken).NotNull().NotEmpty();
            Guard.Argument(sectionProvider).NotNull();

            AccessToken = accessToken;
            SectionProvider = sectionProvider;
            ExceptionHandlingStrategy = ExceptionHandlingStrategy.CATCH;
            NodeId = 0;
            DefaultLocale = null;
        }

        /// <summary>
        /// Sets the general configuration properties to values read from configuration file. Only value which can be set
        /// through <see cref="IConfigurationBuilderBase{T}" /> methods are set.
        /// Any values already set by methods on the current instance are overridden
        /// </summary>
        /// <param name="section">A <see cref="IOddsFeedConfigurationSection"/> from which to load the config</param>
        /// <returns>T.</returns>
        internal virtual void LoadFromConfigFile(IOddsFeedConfigurationSection section)
        {
            Guard.Argument(section).NotNull();

            Section = section;

            if (!string.IsNullOrEmpty(section.SupportedLanguages))
            {
                var langCodes = section.SupportedLanguages.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                SetSupportedLanguages(langCodes.Select(langCode => new CultureInfo(langCode.Trim())));
            }

            if (!string.IsNullOrEmpty(section.DefaultLanguage))
            {
                SetDefaultLanguage(new CultureInfo(section.DefaultLanguage.Trim()));
            }
            ExceptionHandlingStrategy = section.ExceptionHandlingStrategy;
            NodeId = section.NodeId;
            if (!string.IsNullOrEmpty(section.DisabledProducers))
            {
                var producerIds = section.DisabledProducers.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                SetDisabledProducers(producerIds.Select(producerId => int.Parse(producerId.Trim())));
            }
        }

        /// <summary>
        /// Sets the general configuration properties to values read from configuration file. Only value which can be set
        /// through <see cref="T:Sportradar.OddsFeed.SDK.API.IConfigurationBuilderBase`1" /> methods are set. Any values already set by methods on the current instance are overridden
        /// </summary>
        /// <returns>T.</returns>
        public T LoadFromConfigFile()
        {
            LoadFromConfigFile(SectionProvider.GetSection());
            return this as T;
        }

        /// <summary>
        /// Sets the languages in which translatable data is available
        /// </summary>
        /// <param name="cultures">A <see cref="T:System.Collections.Generic.IEnumerable`1" /> specifying languages in which translatable data should be available</param>
        /// <returns>A <see cref="T:Sportradar.OddsFeed.SDK.API.IConfigurationBuilderBase`1" /> derived instance used to set general configuration properties</returns>
        public T SetSupportedLanguages(IEnumerable<CultureInfo> cultures)
        {
            SupportedLocales.Clear();

            if (cultures != null)
            {
                SupportedLocales.AddRange(cultures.Distinct());
            }
            return this as T;
        }

        /// <summary>
        /// Sets the default language in which translatable data is available
        /// </summary>
        /// <param name="culture">A default language in which translatable data should be available</param>
        /// <returns>A <see cref="T:Sportradar.OddsFeed.SDK.API.IConfigurationBuilderBase`1" /> derived instance used to set general configuration properties</returns>
        public T SetDefaultLanguage(CultureInfo culture)
        {
            Guard.Argument(culture).NotNull();

            DefaultLocale = culture;
            return this as T;
        }

        /// <summary>
        /// Sets the value specifying how exceptions thrown in the SDK are handled.
        /// </summary>
        /// <param name="strategy">A <see cref="T:Sportradar.OddsFeed.SDK.Common.ExceptionHandlingStrategy" /> enum specifying how exceptions thrown in the SDK are handled</param>
        /// <returns>A <see cref="T:Sportradar.OddsFeed.SDK.API.IConfigurationBuilderBase`1" /> derived instance used to set general configuration properties</returns>
        public T SetExceptionHandlingStrategy(ExceptionHandlingStrategy strategy)
        {
            ExceptionHandlingStrategy = strategy;
            return this as T;
        }

        /// <summary>
        /// Sets the node id used to separate between SDK instances associated with the same account
        /// </summary>
        /// <param name="nodeId">The node id to be set</param>
        /// <returns>A <see cref="T:Sportradar.OddsFeed.SDK.API.IConfigurationBuilderBase`1" /> derived instance used to set general configuration properties</returns>
        /// <remarks>Use only positive numbers; negative are reserved for internal use</remarks>
        public T SetNodeId(int nodeId)
        {
            NodeId = nodeId;
            return this as T;
        }

        /// <summary>
        /// Specifies the producers which should be disabled (i.e. no recovery, ...)
        /// </summary>
        /// <param name="producerIds">The list of producer ids specifying the producers which should be disabled</param>
        /// <returns>A <see cref="T:Sportradar.OddsFeed.SDK.API.IRecoveryConfigurationBuilder`1" /> derived instance used to set general configuration properties</returns>
        public T SetDisabledProducers(IEnumerable<int> producerIds)
        {
            if (DisabledProducers == null)
            {
                DisabledProducers = new List<int>();
            }
            else
            {
                DisabledProducers.Clear();
            }

            if (producerIds != null)
            {
                DisabledProducers.AddRange(producerIds.Distinct().ToList());
            }
            return this as T;
        }

        /// <summary>
        /// Builds and returns a <see cref="T:Sportradar.OddsFeed.SDK.API.IOddsFeedConfiguration" /> instance
        /// </summary>
        /// <returns>The constructed <see cref="T:Sportradar.OddsFeed.SDK.API.IOddsFeedConfiguration" /> instance</returns>
        public abstract IOddsFeedConfiguration Build();

        /// <summary>
        /// Check the properties values before build the configuration and throws an exception is invalid values are found
        /// </summary>
        /// <exception cref="InvalidOperationException">The value of one or more properties is not correct</exception>
        protected virtual void PreBuildCheck()
        {
            if (DefaultLocale == null && SupportedLocales.Any())
            {
                DefaultLocale = SupportedLocales.First();
            }
            if (!SupportedLocales.Contains(DefaultLocale))
            {
                SupportedLocales.Insert(0, DefaultLocale);
            }

            if (DefaultLocale == null)
            {
                throw new InvalidOperationException("Missing default locale");
            }
            if (SupportedLocales == null || !SupportedLocales.Any())
            {
                throw new InvalidOperationException("Missing supported locales");
            }
            if (string.IsNullOrEmpty(AccessToken))
            {
                throw new InvalidOperationException("Missing access token");
            }
        }
    }
}
