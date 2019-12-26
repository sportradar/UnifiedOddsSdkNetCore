/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Globalization;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract for classes implementing manager info
    /// </summary>
    public interface IManager
    {
        /// <summary>
        /// Gets a <see cref="URN"/> specifying the id of the manager
        /// </summary>
        URN Id { get; }

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
