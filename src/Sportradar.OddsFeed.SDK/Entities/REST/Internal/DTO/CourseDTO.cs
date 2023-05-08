/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-access-object representing a course (used in golf course)
    /// </summary>
    internal class CourseDTO
    {
        public string Id { get; }

        public string Name { get; }

        public ICollection<HoleDTO> Holes { get; }

        internal CourseDTO(course course)
        {
            Id = course.id;
            Name = course.name;
            Holes = course.hole.IsNullOrEmpty()
                ? new List<HoleDTO>()
                : course.hole.Select(s => new HoleDTO(s)).ToList();
        }
    }
}
