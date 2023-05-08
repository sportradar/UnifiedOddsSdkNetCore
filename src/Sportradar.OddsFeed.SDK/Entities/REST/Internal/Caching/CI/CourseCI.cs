/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// A cache item representing a course (used in golf course)
    /// </summary>
    internal class CourseCI
    {
        public string Id { get; }

        public string Name { get; }

        public ICollection<HoleCI> Holes { get; }

        public CourseCI(CourseDTO course)
        {
            Id = course.Id;
            Name = course.Name;
            Holes = course.Holes.IsNullOrEmpty()
                ? new List<HoleCI>()
                : course.Holes.Select(s => new HoleCI(s)).ToList();
        }

        public CourseCI(ExportableCourseCI exportableCourse)
        {
            Id = exportableCourse.Id;
            Name = exportableCourse.Name;
            Holes = exportableCourse.Holes.IsNullOrEmpty()
                ? new List<HoleCI>()
                : exportableCourse.Holes.Select(s => new HoleCI(s)).ToList();
        }
    }
}
