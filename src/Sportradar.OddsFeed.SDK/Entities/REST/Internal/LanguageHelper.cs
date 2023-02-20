/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Castle.Core.Internal;
using Dawn;

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
            var wantedCultureInfos = wantedCultures.ToList();
            Guard.Argument(wantedCultureInfos, nameof(wantedCultures)).NotNull();

            if (alreadyUsedCultures == null)
            {
                return wantedCultureInfos.Distinct();
            }

            var alreadyUsedCultureInfos = alreadyUsedCultures.ToList();
            var missingCultures = wantedCultureInfos.Where(c => !alreadyUsedCultureInfos.Contains(c)).ToList();

            return missingCultures.Distinct();
        }

        /// <summary>
        /// Get string representation of list of cultures
        /// </summary>
        /// <param name="cultures">The list of cultures</param>
        /// <returns>A string representation of list of cultures</returns>
        public static string GetCultureList(IReadOnlyCollection<CultureInfo> cultures)
        {
            return cultures.IsNullOrEmpty() ? string.Empty : string.Join(",", cultures.Select(s => s.TwoLetterISOLanguageName));
        }

        /// <summary>
        /// Get string representation of list of cultures
        /// </summary>
        /// <param name="cultures">The list of cultures</param>
        /// <returns>A string representation of list of cultures</returns>
        public static string GetCultureList(ICollection<CultureInfo> cultures)
        {
            return cultures.IsNullOrEmpty() ? string.Empty : string.Join(",", cultures.Select(s => s.TwoLetterISOLanguageName));
        }
    }
}
