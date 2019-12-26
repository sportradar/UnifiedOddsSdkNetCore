/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// A contract for implementing entity printer
    /// </summary>
    public interface IEntityPrinter
    {
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        string ToString();

        /// <summary>
        /// Returns a string that represents the current object
        /// </summary>
        /// <param name="formatProvider">A format provider used to format the output string</param>
        /// <returns>A string that represents the current object.</returns>
        string ToString(IFormatProvider formatProvider);

        /// <summary>Formats the value of the current instance using the specified format.</summary>
        /// <returns>The value of the current instance in the specified format.</returns>
        /// <param name="format">The format to use.-or- A null reference (Nothing in Visual Basic) to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation. </param>
        /// <param name="formatProvider">The provider to use to format the value.-or- A null reference (Nothing in Visual Basic) to obtain the numeric format information from the current locale setting of the operating system. </param>
        string ToString(string format, IFormatProvider formatProvider = null);
    }
}