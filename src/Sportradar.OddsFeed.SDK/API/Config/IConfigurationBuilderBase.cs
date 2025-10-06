// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Common.Enums;

namespace Sportradar.OddsFeed.SDK.Api.Config
{
    /// <summary>
    /// A base contract custom and general configuration builders
    /// </summary>
    /// <typeparam name="T">The type of the builder extending the interface</typeparam>
    public interface IConfigurationBuilderBase<out T>
    {
        /// <summary>
        /// Sets the general configuration properties to values read from configuration file. Only value which can be set
        /// through <see cref="IConfigurationBuilderBase{T}"/> methods are set. Any values already set by methods on the current instance are overridden
        /// </summary>
        /// <returns>A <see cref="IConfigurationBuilderBase{T}"/> derived instance used to set general configuration properties</returns>
        T LoadFromConfigFile();

        /// <summary>
        /// Sets the languages in which translatable data is available
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{T}"/> specifying languages in which translatable data should be available</param>
        /// <returns>A <see cref="IConfigurationBuilderBase{T}"/> derived instance used to set general configuration properties</returns>
        T SetDesiredLanguages(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Sets the default language in which translatable data is available
        /// </summary>
        /// <param name="culture">A default language in which translatable data should be available</param>
        /// <returns>A <see cref="IConfigurationBuilderBase{T}"/> derived instance used to set general configuration properties</returns>
        T SetDefaultLanguage(CultureInfo culture);

        /// <summary>
        /// Sets the value specifying how exceptions thrown in the SDK are handled.
        /// </summary>
        /// <param name="strategy">A <see cref="ExceptionHandlingStrategy"/> enum specifying how exceptions thrown in the SDK are handled</param>
        /// <returns>A <see cref="IConfigurationBuilderBase{T}"/> derived instance used to set general configuration properties</returns>
        T SetExceptionHandlingStrategy(ExceptionHandlingStrategy strategy);

        /// <summary>
        /// Sets the node id used to separate between SDK instances associated with the same account
        /// </summary>
        /// <param name="nodeId">The node id to be set</param>
        /// <remarks>MTS customer must set this value! Use only positive numbers; negative are reserved for internal use. Each sdk instance should use unique id.</remarks>
        /// <returns>A <see cref="IConfigurationBuilderBase{T}"/> derived instance used to set general configuration properties</returns>
        T SetNodeId(int nodeId);

        /// <summary>
        /// Specifies the producers which should be disabled (i.e. no recovery, ...)
        /// </summary>
        /// <param name="producerIds">The list of producer ids specifying the producers which should be disabled</param>
        /// <returns>A <see cref="IRecoveryConfigurationBuilder{T}"/> derived instance used to set general configuration properties</returns>
        T SetDisabledProducers(IEnumerable<int> producerIds);

        /// <summary>
        /// Builds and returns a <see cref="IUofConfiguration"/> instance
        /// </summary>
        /// <returns>The constructed <see cref="IUofConfiguration"/> instance</returns>
        IUofConfiguration Build();
    }
}
