/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Configuration;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// A <see cref="IConfigurationSectionProvider"/> provider which obtains the section from the app.config file
    /// </summary>
    internal class ConfigurationSectionProvider : IConfigurationSectionProvider
    {
        /// <summary>
        /// A <see cref="object"/> used to ensure thread safety
        /// </summary>
        private readonly object _syncLock = new object();

        /// <summary>
        /// The loaded section.
        /// </summary>
        private IOddsFeedConfigurationSection _section;

        /// <summary>
        /// Gets the <see cref="IOddsFeedConfigurationSection"/>
        /// </summary>
        /// <returns>The <see cref="IOddsFeedConfigurationSection"/></returns>
        /// <exception cref="InvalidOperationException">The configuration could not be loaded or the configuration does not contain the requested section</exception>
        /// <exception cref="ConfigurationErrorsException">The section in the configuration file is not valid</exception>
        public IOddsFeedConfigurationSection GetSection()
        {
            lock (_syncLock)
            {
                if (_section == null)
                {
                    _section = OddsFeedConfigurationSection.GetSection();
                }
            }
            return _section;
        }
    }
}