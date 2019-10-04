/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Provides coverage information
    /// </summary>
    /// <seealso cref="ICoverageInfo" />
    internal class CoverageInfo : EntityPrinter, ICoverageInfoV1
    {
        /// <summary>
        /// The <see cref="Level"/> property backing field
        /// </summary>
        private readonly string _level;

        /// <summary>
        /// The <see cref="IsLive"/> property backing field
        /// </summary>
        private readonly bool _isLive;

        /// <summary>
        /// The <see cref="Includes"/> property backing field
        /// </summary>
        private readonly IReadOnlyCollection<string> _includes;

        /// <summary>
        /// The <see cref="CoveredFrom"/> property backing field
        /// </summary>
        private readonly CoveredFrom? _coveredFrom;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoverageInfo"/> class.
        /// </summary>
        /// <param name="level">a <see cref="string" /> describing the level of the available coverage</param>
        /// <param name="isLive">a value indicating whether the coverage represented by current <see cref="ICoverageInfo" /> is live coverage</param>
        /// <param name="includes">a <see cref="IEnumerable{String}" /> specifying what is included in the coverage represented by the
        /// current <see cref="ICoverageInfo" /> instance</param>
        /// <param name="coveredFrom"></param>
        public CoverageInfo(string level, bool isLive, IEnumerable<string> includes, CoveredFrom? coveredFrom)
        {
            _level = level;
            _isLive = isLive;
            if (includes != null)
            {
                _includes = includes as IReadOnlyCollection<string> ?? new ReadOnlyCollection<string>(includes.ToList());
            }
            _coveredFrom = coveredFrom;
        }

        public CoverageInfo(CoverageInfoDTO dto)
            :this(dto.Level, dto.IsLive, dto.Includes, dto.CoveredFrom)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoverageInfo"/> class.
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableCoverageInfoCI" /> specifying the current item</param>
        public CoverageInfo(ExportableCoverageInfoCI exportable)
        {
            if (exportable == null)
                throw new ArgumentNullException(nameof(exportable));

            _level = exportable.Level;
            _isLive = exportable.IsLive;
            _includes = exportable.Includes != null ? new List<string>(exportable.Includes) : null;
            _coveredFrom = exportable.CoveredFrom;
        }

        /// <summary>
        /// Gets a <see cref="string" /> describing the level of the available coverage
        /// </summary>
        public string Level => _level;

        /// <summary>
        /// Gets a value indicating whether the coverage represented by current <see cref="ICoverageInfo" /> is live coverage
        /// </summary>
        public bool IsLive => _isLive;

        /// <summary>
        /// Gets a <see cref="IEnumerable{String}" /> specifying what is included in the coverage represented by the
        /// current <see cref="ICoverageInfo" /> instance
        /// </summary>
        public IEnumerable<string> Includes => _includes;

        /// <summary>
        /// Gets a <see cref="CoveredFrom"/> describing the coverage location
        /// </summary>
        public CoveredFrom? CoveredFrom => _coveredFrom;

        /// <summary>
        /// Constructs and returns a <see cref="string"/> containing the id of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing the id of the current instance.</returns>
        protected override string PrintI()
        {
            return $"Level={_level}";
        }

        /// <summary>
        /// Constructs and returns a <see cref="string"/> containing compacted representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing compacted representation of the current instance.</returns>
        protected override string PrintC()
        {
            var names = string.Join(", ", _includes);
            return $"Level={_level}, IsLive={_isLive}, Includes=[{names}], CoveredFrom={CoveredFrom}";
        }

        /// <summary>
        /// Constructs and return a <see cref="string"/> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing details of the current instance.</returns>
        protected override string PrintF()
        {
            return PrintC();
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing a JSON representation of the current instance
        /// </summary>
        /// <returns>a <see cref="string" /> containing a JSON representation of the current instance.</returns>
        protected override string PrintJ()
        {
            return PrintJ(GetType(), this);
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public Task<ExportableCoverageInfoCI> ExportAsync()
        {
            return Task.FromResult(new ExportableCoverageInfoCI
            {
                Includes = new List<string>(_includes ?? new List<string>()),
                IsLive = _isLive,
                CoveredFrom = _coveredFrom,
                Level = _level
            });
        }
    }
}
