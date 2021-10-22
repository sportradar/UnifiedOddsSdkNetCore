/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Configuration;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Represents a contract implemented by classes used to provide access to <see cref="IOddsFeedConfigurationSection"/> instance
    /// </summary>
    internal interface IConfigurationSectionProvider
    {
        /// <summary>
        /// Gets the <see cref="IOddsFeedConfigurationSection"/>
        /// </summary>
        /// <returns>The <see cref="IOddsFeedConfigurationSection"/></returns>
        /// <exception cref="InvalidOperationException">The configuration could not be loaded or the configuration does not contain the requested section</exception>
        /// <exception cref="ConfigurationErrorsException">The section in the configuration file is not valid</exception>
        IOddsFeedConfigurationSection GetSection();
    }
}