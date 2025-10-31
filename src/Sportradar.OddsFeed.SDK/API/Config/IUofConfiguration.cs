// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest;

namespace Sportradar.OddsFeed.SDK.Api.Config
{
    /// <summary>
    /// Defines a contract implemented by classes representing uof sdk configuration / settings
    /// </summary>
    public interface IUofConfiguration
    {
        /// <summary>
        /// Gets the access token used when accessing feed's REST interface
        /// </summary>
        string AccessToken { get; }

        /// <summary>
        /// Gets the <see cref="SdkEnvironment"/> value specifying the environment to which to connect.
        /// </summary>
        SdkEnvironment Environment { get; }

        /// <summary>
        /// Gets the node identifier
        /// </summary>
        /// <remarks>MTS customer must set this value! Use only positive numbers; negative are reserved for internal use.</remarks>
        int NodeId { get; }

        /// <summary>
        /// Gets a <see cref="CultureInfo"/> specifying default languages to which translatable values will be translated
        /// </summary>
        CultureInfo DefaultLanguage { get; }

        /// <summary>
        /// Gets a <see cref="IEnumerable{CultureInfo}"/> specifying languages to which translatable values will be translated
        /// </summary>
        List<CultureInfo> Languages { get; }

        /// <summary>
        /// Gets the exception handling strategy
        /// </summary>
        ExceptionHandlingStrategy ExceptionHandlingStrategy { get; }

        /// <summary>
        /// Gets the bookmaker details
        /// </summary>
        /// <value>The bookmaker details</value>
        IBookmakerDetails BookmakerDetails { get; }

        /// <summary>
        /// Gets the settings used for rabbit connection
        /// </summary>
        IUofRabbitConfiguration Rabbit { get; }

        /// <summary>
        /// Gets the settings used for Sports API connection
        /// </summary>
        IUofApiConfiguration Api { get; }

        /// <summary>
        /// Gets the settings used for producer handling
        /// </summary>
        IUofProducerConfiguration Producer { get; }

        /// <summary>
        /// Gets the settings used for sdk caches
        /// </summary>
        IUofCacheConfiguration Cache { get; }

        /// <summary>
        /// Gets the additional settings
        /// </summary>
        IUofAdditionalConfiguration Additional { get; }

        /// <summary>
        /// Gets the settings for usage exporter
        /// </summary>
        IUofUsageConfiguration Usage { get; }
    }
}
