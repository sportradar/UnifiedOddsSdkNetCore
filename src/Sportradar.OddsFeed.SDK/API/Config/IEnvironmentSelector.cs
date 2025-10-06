// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Common.Enums;

namespace Sportradar.OddsFeed.SDK.Api.Config
{
    /// <summary>
    /// Defines a contract implemented by classes taking care of the 2nd step when building configuration - selecting the environment.
    /// </summary>
    public interface IEnvironmentSelector
    {
        /// <summary>
        /// Returns a <see cref="IConfigurationBuilder"/> with properties set to values needed to access replay server
        /// </summary>
        /// <returns>A <see cref="IConfigurationBuilder"/> with properties set to values needed to access replay server</returns>
        IConfigurationBuilder SelectReplay();

        /// <summary>
        /// Returns a <see cref="ICustomConfigurationBuilder"/> allowing the properties to be set to custom values (useful for testing with non-standard AMQP)
        /// </summary>
        /// <returns>A <see cref="ICustomConfigurationBuilder"/> with properties set to values needed to access replay server</returns>
        ICustomConfigurationBuilder SelectCustom();

        /// <summary>
        /// Returns a <see cref="IConfigurationBuilder"/> with properties set to values needed to access specified environment
        /// </summary>
        /// <remarks>For accessing replay or custom server use SelectReplay or SelectCustom</remarks>
        /// <param name="ufEnvironment">A <see cref="SdkEnvironment"/> specifying to which environment to connect</param>
        /// <returns>A <see cref="IConfigurationBuilder"/> with properties set to values needed to access production environment</returns>
        IConfigurationBuilder SelectEnvironment(SdkEnvironment ufEnvironment);

        /// <summary>
        /// Returns a <see cref="IConfigurationBuilder"/> with properties set to values needed to access specified environment
        /// </summary>
        /// <remarks>For accessing replay or custom server use SelectReplay or SelectCustom</remarks>
        /// <returns>A <see cref="IConfigurationBuilder"/> with properties set to values needed to access predefined environment</returns>
        IConfigurationBuilder SelectEnvironmentFromConfigFile();
    }
}
