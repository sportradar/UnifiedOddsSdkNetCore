// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Configuration;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Config
{
    /// <summary>
    /// A <see cref="IUofConfigurationSectionProvider"/> provider which obtains the section from the app.config file
    /// </summary>
    internal class UofConfigurationSectionProvider : IUofConfigurationSectionProvider
    {
        /// <summary>
        /// A <see cref="object"/> used to ensure thread safety
        /// </summary>
        private readonly object _syncLock = new object();

        /// <summary>
        /// The loaded section.
        /// </summary>
        private IUofConfigurationSection _section;

        /// <summary>
        /// Gets the <see cref="IUofConfigurationSection"/>
        /// </summary>
        /// <returns>The <see cref="IUofConfigurationSection"/></returns>
        /// <exception cref="InvalidOperationException">The configuration could not be loaded or the configuration does not contain the requested section</exception>
        /// <exception cref="ConfigurationErrorsException">The section in the configuration file is not valid</exception>
        public IUofConfigurationSection GetSection()
        {
            lock (_syncLock)
            {
                if (_section == null)
                {
                    _section = UofConfigurationSection.GetSection();
                }
            }
            return _section;
        }
    }
}
