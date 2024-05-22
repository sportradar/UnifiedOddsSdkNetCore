// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Dawn;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Config
{
    /// <summary>
    /// Class EnvironmentSelector
    /// </summary>
    /// <seealso cref="IEnvironmentSelector" />
    internal class EnvironmentSelector : IEnvironmentSelector
    {
        private readonly IUofConfigurationSectionProvider _sectionProvider;

        private readonly IBookmakerDetailsProvider _bookmakerDetailsProvider;

        private readonly IProducersProvider _producersProvider;

        private readonly UofConfiguration _configuration;

        /// <summary>
        /// Constructs a new instance of the <see cref="EnvironmentSelector"/> class
        /// </summary>
        /// <param name="configuration">Current <see cref="UofConfiguration"/></param>
        /// <param name="sectionProvider">A <see cref="IUofConfigurationSectionProvider"/> used to access <see cref="IUofConfigurationSection"/></param>
        /// <param name="bookmakerDetailsProvider">Provider for bookmaker details</param>
        /// <param name="producersProvider">Provider for available producers</param>
        // ReSharper disable once TooManyDependencies
        internal EnvironmentSelector(UofConfiguration configuration,
            IUofConfigurationSectionProvider sectionProvider,
            IBookmakerDetailsProvider bookmakerDetailsProvider,
            IProducersProvider producersProvider)
        {
            Guard.Argument(configuration, nameof(configuration)).NotNull();
            Guard.Argument(sectionProvider, nameof(sectionProvider)).NotNull();

            _configuration = configuration;
            _sectionProvider = sectionProvider;
            _bookmakerDetailsProvider = bookmakerDetailsProvider;
            _producersProvider = producersProvider;
        }

        public IConfigurationBuilder SelectReplay()
        {
            return new ConfigurationBuilder(_configuration, _sectionProvider, SdkEnvironment.Replay, _bookmakerDetailsProvider, _producersProvider);
        }

        public ICustomConfigurationBuilder SelectCustom()
        {
            return new CustomConfigurationBuilder(_configuration, _sectionProvider, _bookmakerDetailsProvider, _producersProvider);
        }

        public IConfigurationBuilder SelectEnvironment(SdkEnvironment ufEnvironment)
        {
            if (ufEnvironment == SdkEnvironment.Custom)
            {
                throw new InvalidOperationException("Use SelectCustom() for custom environment.");
            }
            return new ConfigurationBuilder(_configuration, _sectionProvider, ufEnvironment, _bookmakerDetailsProvider, _producersProvider);
        }

        public IConfigurationBuilder SelectEnvironmentFromConfigFile()
        {
            var section = _sectionProvider.GetSection();
            if (section == null)
            {
                throw new InvalidOperationException("Missing configuration section");
            }
            return SelectEnvironment(section.Environment);
        }
    }
}
