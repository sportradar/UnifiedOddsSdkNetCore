// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Globalization;
using Sportradar.OddsFeed.SDK.Common;

namespace Sportradar.OddsFeed.SDK.Entities.Rest
{
    /// <summary>
    /// Defines a contract for classes implementing manager info
    /// </summary>
    //TODO: manager missing Names as Venue for example
    public interface IManager
    {
        /// <summary>
        /// Gets a <see cref="Urn"/> specifying the id of the manager
        /// </summary>
        Urn Id { get; }

        /// <summary>
        /// Gets the name of the manager
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the returned name</param>
        /// <returns>Return a name of the manager</returns>
        string GetName(CultureInfo culture);

        /// <summary>
        /// Gets the nationality of the manager
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the returned nationality</param>
        /// <returns>Return a nationality of the manager</returns>
        string GetNationality(CultureInfo culture);

        /// <summary>
        /// Gets the country code
        /// </summary>
        /// <value>The country code</value>
        string CountryCode { get; }
    }
}
