// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using Dawn;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Extensions;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Config
{
    /// <summary>
    /// Represents a base class for configuration builders
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal abstract class ConfigurationBuilderBase<T> : IConfigurationBuilderBase<T> where T : class
    {
        private const string CommonIamFailureMessage = "Configuration build failed: Common IAM token acquisition failed.";

        /// <summary>
        /// The <see cref="IUofConfigurationSectionProvider"/> containing data read from the config file
        /// </summary>
        internal readonly IUofConfigurationSectionProvider SectionProvider;

        private readonly Func<IUofConfiguration, IBookmakerDetailsProvider> _bookmakerDetailsProviderFactory;

        private readonly Func<IUofConfiguration, IProducersProvider> _producersProviderFactory;

        internal readonly UofConfiguration UofConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationBuilderBase{T}"/> class
        /// </summary>
        /// <param name="configuration">Current <see cref="UofConfiguration"/></param>
        /// <param name="sectionProvider">A <see cref="IUofConfigurationSectionProvider"/> used to access <see cref="IUofConfigurationSection"/></param>
        /// <param name="bookmakerDetailsProviderFactory">Factory invoked to obtain a <see cref="IBookmakerDetailsProvider"/></param>
        /// <param name="producersProviderFactory">Factory invoked to obtain a <see cref="IProducersProvider"/></param>
        private protected ConfigurationBuilderBase(UofConfiguration configuration,
                                                   IUofConfigurationSectionProvider sectionProvider,
                                                   Func<IUofConfiguration, IBookmakerDetailsProvider> bookmakerDetailsProviderFactory,
                                                   Func<IUofConfiguration, IProducersProvider> producersProviderFactory)
        {
            UofConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            SectionProvider = sectionProvider ?? throw new ArgumentNullException(nameof(sectionProvider));
            _bookmakerDetailsProviderFactory = bookmakerDetailsProviderFactory;
            _producersProviderFactory = producersProviderFactory;
        }

        /// <summary>
        /// Sets the general configuration properties to values read from configuration file. Only value which can be set
        /// through <see cref="IConfigurationBuilderBase{T}" /> methods are set. Any values already set by methods on the current instance are overridden
        /// </summary>
        /// <returns>A <see cref="IConfigurationBuilderBase{T}" /> derived instance used to set general configuration properties</returns>
        public virtual T LoadFromConfigFile()
        {
            UofConfiguration.UpdateFromAppConfigSection(false);
            return this as T;
        }

        /// <summary>
        /// Sets the languages in which translatable data is available
        /// </summary>
        /// <param name="cultures">A list of languages in which translatable data should be available</param>
        /// <returns>A <see cref="IConfigurationBuilderBase{T}" /> derived instance used to set general configuration properties</returns>
        public T SetDesiredLanguages(IEnumerable<CultureInfo> cultures)
        {
            UofConfiguration.Languages.Clear();

            if (!cultures.IsNullOrEmpty())
            {
                UofConfiguration.Languages.AddRange(cultures.Distinct());
            }
            return this as T;
        }

        /// <summary>
        /// Sets the default language in which translatable data is available
        /// </summary>
        /// <param name="culture">A default language in which translatable data should be available</param>
        /// <returns>A <see cref="IConfigurationBuilderBase{T}" /> derived instance used to set general configuration properties</returns>
        public T SetDefaultLanguage(CultureInfo culture)
        {
            Guard.Argument(culture, nameof(culture)).NotNull();

            UofConfiguration.DefaultLanguage = culture;
            return this as T;
        }

        /// <summary>
        /// Sets the value specifying how exceptions thrown in the SDK are handled.
        /// </summary>
        /// <param name="strategy">A <see cref="ExceptionHandlingStrategy" /> enum specifying how exceptions thrown in the SDK are handled</param>
        /// <returns>A <see cref="IConfigurationBuilder" /> derived instance used to set general configuration properties</returns>
        public T SetExceptionHandlingStrategy(ExceptionHandlingStrategy strategy)
        {
            UofConfiguration.ExceptionHandlingStrategy = strategy;
            return this as T;
        }

        /// <summary>
        /// Sets the node id used to separate between SDK instances associated with the same account
        /// </summary>
        /// <param name="nodeId">The node id to be set</param>
        /// <returns>A <see cref="IConfigurationBuilder" /> derived instance used to set general configuration properties</returns>
        /// <remarks>Use only positive numbers; negative are reserved for internal use.</remarks>
        public T SetNodeId(int nodeId)
        {
            UofConfiguration.NodeId = nodeId;
            return this as T;
        }

        /// <summary>
        /// Specifies the producers which should be disabled (i.e. no recovery, ...)
        /// </summary>
        /// <param name="producerIds">The list of producer ids specifying the producers which should be disabled</param>
        /// <returns>A <see cref="IConfigurationBuilder" /> derived instance used to set general configuration properties</returns>
        public T SetDisabledProducers(IEnumerable<int> producerIds)
        {
            UofConfiguration.Producer.DisabledProducers.Clear();

            if (!producerIds.IsNullOrEmpty())
            {
                UofConfiguration.Producer.DisabledProducers.AddRange(producerIds.Distinct().ToList());
            }
            return this as T;
        }

        /// <summary>
        /// Builds and returns a <see cref="IUofConfiguration" /> instance
        /// </summary>
        /// <returns>The constructed <see cref="IUofConfiguration" /> instance</returns>
        public abstract IUofConfiguration Build();

        /// <summary>
        /// Check the properties values before build the configuration and throws an exception is invalid values are found
        /// </summary>
        /// <exception cref="InvalidOperationException">The value of one or more properties is not correct</exception>
        protected virtual void PreBuildCheck()
        {
            UofConfiguration.ValidateMinimumSettings();
        }

        protected void FetchBookmakerDetails()
        {
            try
            {
                var bookmakerDetailsProvider = _bookmakerDetailsProviderFactory(UofConfiguration);
                bookmakerDetailsProvider.LoadBookmakerDetails(UofConfiguration);
            }
            catch (CommunicationException ex) when (IsCommonIamTokenAcquisitionFailure(ex))
            {
                throw CreateCommonIamFailureException(ex);
            }
            catch (CommunicationException ex)
            {
                throw new CommunicationException("Fetching bookmaker details failed.", null, ex);
            }
        }

        protected void FetchProducers()
        {
            try
            {
                var producersProvider = _producersProviderFactory(UofConfiguration);
                var producers = producersProvider.GetProducers();
                ((UofProducerConfiguration)UofConfiguration.Producer).Producers = producers.ToList();
            }
            catch (CommunicationException ex) when (IsCommonIamTokenAcquisitionFailure(ex))
            {
                throw CreateCommonIamFailureException(ex);
            }
            catch (CommunicationException ex)
            {
                throw new CommunicationException("Fetching producers data failed.", null, ex);
            }
        }

        private static CommunicationException CreateCommonIamFailureException(CommunicationException commonIamException)
        {
            return new CommunicationException(CommonIamFailureMessage, commonIamException.Url, commonIamException.ResponseCode, commonIamException);
        }

        private static bool IsCommonIamTokenAcquisitionFailure(CommunicationException exception)
        {
            return exception.ResponseCode == HttpStatusCode.Unauthorized &&
                   exception.Message.Equals("Authentication token is not available. Unable to authenticate the request.", StringComparison.OrdinalIgnoreCase);
        }
    }
}
