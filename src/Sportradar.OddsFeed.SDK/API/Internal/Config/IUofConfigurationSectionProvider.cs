// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Configuration;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Config
{
    /// <summary>
    /// Represents a contract implemented by classes used to provide access to <see cref="IUofConfigurationSection"/> instance
    /// </summary>
    internal interface IUofConfigurationSectionProvider
    {
        /// <summary>
        /// Gets the <see cref="IUofConfigurationSection"/>
        /// </summary>
        /// <returns>The <see cref="IUofConfigurationSection"/></returns>
        /// <exception cref="InvalidOperationException">The configuration could not be loaded or the configuration does not contain the requested section</exception>
        /// <exception cref="ConfigurationErrorsException">The section in the configuration file is not valid</exception>
        IUofConfigurationSection GetSection();
    }
}
