/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Dawn;
using System.Globalization;
using System.Linq;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal
{
    internal static class LanguageHelper
    {
        /// <summary>
        /// Returns a <see cref="IEnumerable{CultureInfo}"/> representing the missing languages
        /// </summary>
        /// <param name="wantedCultures">A <see cref="IEnumerable{CultureInfo}"/> containing list of <see cref="CultureInfo"/> we want to get</param>
        /// <param name="alreadyUsedCultures">A <see cref="IEnumerable{CultureInfo}"/> specifying cultures that was already used/fetched</param>
        /// <returns>A <see cref="IEnumerable{CultureInfo}"/> containing missing cultures or a empty list if no cultures are missing</returns>
        internal static IEnumerable<CultureInfo> GetMissingCultures(IEnumerable<CultureInfo> wantedCultures, IEnumerable<CultureInfo> alreadyUsedCultures)
        {
            Guard.Argument(wantedCultures).NotNull().NotEmpty();

            if (alreadyUsedCultures == null)
            {
                return wantedCultures.Distinct();
            }

            var missingCultures = wantedCultures.Where(c => !alreadyUsedCultures.Contains(c)).ToList();

            return missingCultures.Distinct();
        }
    }
}
