// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Api.Config
{
    /// <summary>
    /// Defines a contract implemented by classes used to set general configuration properties
    /// </summary>
    /// <remarks>
    /// Types associated with <see cref="IConfigurationBuilder"/> represent a re-factored approach to building SDK configuration and
    /// therefore make <see cref="IConfigurationBuilder"/> related instances obsolete. The <see cref="IConfigurationBuilder"/>
    /// and related instances cannot be removed in order not to introduce braking changes.
    /// </remarks>
    public interface IConfigurationBuilder : IRecoveryConfigurationBuilder<IConfigurationBuilder>
    {
    }
}
