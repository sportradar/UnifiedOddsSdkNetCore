// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-access-object representing a course (used in golf course)
    /// </summary>
    internal class CourseDto
    {
        /// <summary>
        /// Gets a <see cref="Urn"/> representing the id of the represented course
        /// </summary>
        /// <value>The identifier</value>
        public Urn Id { get; }

        /// <summary>
        /// Gets the name of the represented course
        /// </summary>
        /// <value>The name</value>
        public string Name { get; }

        public ICollection<HoleDto> Holes { get; }

        internal CourseDto(course course)
        {
            if (!course.id.IsNullOrEmpty())
            {
                Id = Urn.Parse(course.id);
            }
            Name = course.name;
            Holes = course.hole.IsNullOrEmpty()
                ? new List<HoleDto>()
                : course.hole.Select(s => new HoleDto(s)).ToList();
        }
    }
}
