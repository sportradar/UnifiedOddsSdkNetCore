/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Globalization;

namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Defines methods used to access outcome definition properties
    /// </summary>
    public interface IOutcomeDefinition
    {
        /// <summary>
        /// Returns the unmodified outcome name template
        /// </summary>
        /// <param name="culture">The culture in which the name template should be provided</param>
        /// <returns>The unmodified name template</returns>
        string GetNameTemplate(CultureInfo culture);
    }
}
