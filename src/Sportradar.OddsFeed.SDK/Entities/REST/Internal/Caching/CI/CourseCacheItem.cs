/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI
{
    /// <summary>
    /// A cache item representing a course (used in golf course)
    /// </summary>
    internal class CourseCacheItem
    {
        public Urn Id { get; }

        public IDictionary<CultureInfo, string> Names { get; }

        public ICollection<HoleCacheItem> Holes { get; private set; }

        public CourseCacheItem(CourseDto courseDto, CultureInfo culture)
        {
            Id = courseDto.Id;
            Names = new Dictionary<CultureInfo, string>();
            Holes = new List<HoleCacheItem>();

            Merge(courseDto, culture);
        }

        public CourseCacheItem(ExportableCourse exportableCourse)
        {
            if (!exportableCourse.Id.IsNullOrEmpty())
            {
                Id = Urn.Parse(exportableCourse.Id);
            }
            Names = exportableCourse.Names ?? new Dictionary<CultureInfo, string>();
            Holes = exportableCourse.Holes.IsNullOrEmpty()
                ? new List<HoleCacheItem>()
                : exportableCourse.Holes.Select(s => new HoleCacheItem(s)).ToList();
        }

        public void Merge(CourseDto courseDto, CultureInfo culture)
        {
            if (!courseDto.Name.IsNullOrEmpty())
            {
                Names[culture] = courseDto.Name;
            }

            if (!courseDto.Holes.IsNullOrEmpty())
            {
                Holes.Clear();
                Holes = courseDto.Holes.Select(s => new HoleCacheItem(s)).ToList();
            }
        }

        public ExportableCourse Export()
        {
            var exportable = new ExportableCourse { Id = Id?.ToString(), Names = Names, Holes = Holes.Select(s => new ExportableHole { Number = s.Number, Par = s.Par }).ToList() };

            return exportable;
        }
    }
}
