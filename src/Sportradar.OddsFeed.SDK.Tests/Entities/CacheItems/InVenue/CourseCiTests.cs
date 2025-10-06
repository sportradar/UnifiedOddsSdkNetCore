// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InVenue;

public class CourseCiTests : VenueHelper
{
    [Fact]
    public void Hole_Constructor()
    {
        var hole = new hole { number = 10, par = 5 };
        var holeCi = new HoleCacheItem(new HoleDto(hole));

        Assert.NotNull(holeCi);
        Assert.Equal(10, holeCi.Number);
        Assert.Equal(5, holeCi.Par);
    }

    [Fact]
    public void Hole_ConstructorWithNumberOnly()
    {
        var hole = new hole { number = 10 };
        var holeCi = new HoleCacheItem(new HoleDto(hole));

        Assert.NotNull(holeCi);
        Assert.Equal(10, holeCi.Number);
        Assert.Equal(0, holeCi.Par);
    }

    [Fact]
    public void Hole_ConstructorWithNegativeNumber()
    {
        var hole = new hole { number = -10 };
        var holeCi = new HoleCacheItem(new HoleDto(hole));

        Assert.NotNull(holeCi);
        Assert.Equal(-10, holeCi.Number);
        Assert.Equal(0, holeCi.Par);
    }

    [Fact]
    public void Hole_ConstructorWithParOnly()
    {
        var hole = new hole { par = 5 };
        var holeCi = new HoleCacheItem(new HoleDto(hole));

        Assert.NotNull(holeCi);
        Assert.Equal(0, holeCi.Number);
        Assert.Equal(5, holeCi.Par);
    }

    [Fact]
    public void Hole_ConstructorWithNegativePar()
    {
        var hole = new hole { par = -5 };
        var holeCi = new HoleCacheItem(new HoleDto(hole));

        Assert.NotNull(holeCi);
        Assert.Equal(0, holeCi.Number);
        Assert.Equal(-5, holeCi.Par);
    }

    [Fact]
    public void Hole_ConstructorWithNoData()
    {
        var hole = new hole();
        var holeCi = new HoleCacheItem(new HoleDto(hole));

        Assert.NotNull(holeCi);
        Assert.Equal(0, holeCi.Number);
        Assert.Equal(0, holeCi.Par);
    }

    [Fact]
    public void Constructor_Normal()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseFull(1)), TestData.Culture);

        Assert.NotNull(courseCi);
        Assert.Equal(DefaultCourseId, courseCi.Id);
        Assert.Single(courseCi.Names);
        Assert.Equal(DefaultCourseName, courseCi.Names[TestData.Culture]);
        ValidateHoles(courseCi.Holes, 1);
    }

    [Fact]
    public void Constructor_With9Holes()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseFull()), TestData.Culture);

        Assert.NotNull(courseCi);
        Assert.Equal(DefaultCourseId, courseCi.Id);
        Assert.Single(courseCi.Names);
        Assert.Equal(DefaultCourseName, courseCi.Names[TestData.Culture]);
        ValidateHoles(courseCi.Holes);
    }

    [Fact]
    public void Constructor_WithoutId()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseWithoutId(1)), TestData.Culture);

        Assert.NotNull(courseCi);
        Assert.Null(courseCi.Id);
        Assert.Single(courseCi.Names);
        Assert.Equal(DefaultCourseName, courseCi.Names[TestData.Culture]);
        ValidateHoles(courseCi.Holes, 1);
    }

    [Fact]
    public void Constructor_WithoutName()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseWithoutName(1)), TestData.Culture);

        Assert.NotNull(courseCi);
        Assert.Equal(DefaultCourseId, courseCi.Id);
        Assert.Empty(courseCi.Names);
        ValidateHoles(courseCi.Holes, 1);
    }

    [Fact]
    public void Constructor_WithoutHoles()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseWithoutHoles()), TestData.Culture);

        Assert.NotNull(courseCi);
        Assert.Equal(DefaultCourseId, courseCi.Id);
        Assert.Single(courseCi.Names);
        Assert.Equal(DefaultCourseName, courseCi.Names[TestData.Culture]);
        Assert.Empty(courseCi.Holes);
    }

    [Fact]
    public void ConstructorExportable_Normal()
    {
        var exportableCourse = new ExportableCourse
        {
            Id = DefaultCourseId.ToString(),
            Names = new Dictionary<CultureInfo, string> { { TestData.Culture, DefaultCourseName } },
            Holes = GetExportableHoles(1)
        };

        var courseCi = new CourseCacheItem(exportableCourse);

        Assert.NotNull(courseCi);
        Assert.Equal(DefaultCourseId, courseCi.Id);
        Assert.Single(courseCi.Names);
        Assert.Equal(DefaultCourseName, courseCi.Names[TestData.Culture]);
        ValidateHoles(courseCi.Holes, 1);
    }

    [Fact]
    public void ConstructorExportable_With9Holes()
    {
        var exportableCourse = new ExportableCourse
        {
            Id = DefaultCourseId.ToString(),
            Names = new Dictionary<CultureInfo, string> { { TestData.Culture, DefaultCourseName } },
            Holes = GetExportableHoles()
        };

        var courseCi = new CourseCacheItem(exportableCourse);

        Assert.NotNull(courseCi);
        Assert.Equal(DefaultCourseId, courseCi.Id);
        Assert.Single(courseCi.Names);
        Assert.Equal(DefaultCourseName, courseCi.Names[TestData.Culture]);
        ValidateHoles(courseCi.Holes);
    }

    [Fact]
    public void ConstructorExportable_WithoutId()
    {
        var exportableCourse = new ExportableCourse
        {
            Names = new Dictionary<CultureInfo, string> { { TestData.Culture, DefaultCourseName } },
            Holes = GetExportableHoles(1)
        };

        var courseCi = new CourseCacheItem(exportableCourse);

        Assert.NotNull(courseCi);
        Assert.Null(courseCi.Id);
        Assert.Single(courseCi.Names);
        Assert.Equal(DefaultCourseName, courseCi.Names[TestData.Culture]);
        ValidateHoles(courseCi.Holes, 1);
    }

    [Fact]
    public void ConstructorExportable_WithoutName()
    {
        var exportableCourse = new ExportableCourse
        {
            Id = DefaultCourseId.ToString(),
            Holes = GetExportableHoles(1)
        };

        var courseCi = new CourseCacheItem(exportableCourse);

        Assert.NotNull(courseCi);
        Assert.Equal(DefaultCourseId, courseCi.Id);
        Assert.Empty(courseCi.Names);
        ValidateHoles(courseCi.Holes, 1);
    }

    [Fact]
    public void ConstructorExportable_WithoutHoles()
    {
        var exportableCourse = new ExportableCourse
        {
            Id = DefaultCourseId.ToString(),
            Names = new Dictionary<CultureInfo, string> { { TestData.Culture, DefaultCourseName } }
        };

        var courseCi = new CourseCacheItem(exportableCourse);

        Assert.NotNull(courseCi);
        Assert.Equal(DefaultCourseId, courseCi.Id);
        Assert.Single(courseCi.Names);
        Assert.Equal(DefaultCourseName, courseCi.Names[TestData.Culture]);
        ValidateHoles(courseCi.Holes, 0);
    }

    [Fact]
    public void Export_Normal()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseFull(1)), TestData.Culture);
        var exportable = courseCi.Export();

        Assert.NotNull(exportable);
        Assert.Equal(DefaultCourseId.ToString(), exportable.Id);
        Assert.Single(exportable.Names);
        Assert.Equal(DefaultCourseName, exportable.Names[TestData.Culture]);
        Assert.Single(exportable.Holes);
        Assert.Equal(1, exportable.Holes.First().Number);
        Assert.Equal(4, exportable.Holes.First().Par);
    }

    [Fact]
    public void Export_With9Holes()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseFull()), TestData.Culture);
        var exportable = courseCi.Export();

        Assert.NotNull(exportable);
        Assert.Equal(DefaultCourseId.ToString(), exportable.Id);
        Assert.Single(exportable.Names);
        Assert.Equal(DefaultCourseName, exportable.Names[TestData.Culture]);
        Assert.Equal(9, exportable.Holes.Count);

        for (var i = 1; i <= 9; i++)
        {
            Assert.Equal(i, exportable.Holes.ElementAt(i - 1).Number);
            Assert.Equal(4, exportable.Holes.ElementAt(i - 1).Par);
        }
    }

    [Fact]
    public void Export_WithoutId()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseWithoutId(1)), TestData.Culture);
        var exportable = courseCi.Export();

        Assert.NotNull(exportable);
        Assert.Null(exportable.Id);
        Assert.Single(exportable.Names);
        Assert.Equal(DefaultCourseName, exportable.Names[TestData.Culture]);
        Assert.Single(exportable.Holes);
        Assert.Equal(1, exportable.Holes.First().Number);
        Assert.Equal(4, exportable.Holes.First().Par);
    }

    [Fact]
    public void Export_WithoutName()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseWithoutName(1)), TestData.Culture);
        var exportable = courseCi.Export();

        Assert.NotNull(exportable);
        Assert.Equal(DefaultCourseId.ToString(), exportable.Id);
        Assert.Empty(courseCi.Names);
        Assert.Single(exportable.Holes);
        Assert.Equal(1, exportable.Holes.First().Number);
        Assert.Equal(4, exportable.Holes.First().Par);
    }

    [Fact]
    public void Export_WithoutHoles()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseWithoutHoles()), TestData.Culture);
        var exportable = courseCi.Export();

        Assert.NotNull(exportable);
        Assert.Equal(DefaultCourseId.ToString(), exportable.Id);
        Assert.Equal(DefaultCourseName, exportable.Names[TestData.Culture]);
        Assert.Empty(exportable.Holes);
    }

    [Fact]
    public void Merge_Normal()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseFull(1)), TestData.Culture);
        var sapiCourse = GetApiCourseFull(1);
        sapiCourse.name = "New Name";
        courseCi.Merge(new CourseDto(sapiCourse), TestData.CultureNl);

        Assert.NotNull(courseCi);
        Assert.Equal(DefaultCourseId, courseCi.Id);
        Assert.Equal(2, courseCi.Names.Count);
        Assert.Equal(DefaultCourseName, courseCi.Names[TestData.Culture]);
        Assert.Equal(sapiCourse.name, courseCi.Names[TestData.CultureNl]);
        ValidateHoles(courseCi.Holes, 1);
    }

    [Fact]
    public void Merge_With9Holes()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseFull()), TestData.Culture);
        var sapiCourse = GetApiCourseFull();
        sapiCourse.name = "New Name";
        courseCi.Merge(new CourseDto(sapiCourse), TestData.CultureNl);

        Assert.NotNull(courseCi);
        Assert.Equal(DefaultCourseId, courseCi.Id);
        Assert.Equal(2, courseCi.Names.Count);
        Assert.Equal(DefaultCourseName, courseCi.Names[TestData.Culture]);
        Assert.Equal(sapiCourse.name, courseCi.Names[TestData.CultureNl]);
        ValidateHoles(courseCi.Holes);
    }

    [Fact]
    public void Merge_WithoutId()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseWithoutId(1)), TestData.Culture);
        var sapiCourse = GetApiCourseWithoutId(1);
        sapiCourse.name = "New Name";
        courseCi.Merge(new CourseDto(sapiCourse), TestData.CultureNl);

        Assert.NotNull(courseCi);
        Assert.Null(courseCi.Id);
        Assert.Equal(2, courseCi.Names.Count);
        Assert.Equal(DefaultCourseName, courseCi.Names[TestData.Culture]);
        Assert.Equal(sapiCourse.name, courseCi.Names[TestData.CultureNl]);
        ValidateHoles(courseCi.Holes, 1);
    }

    [Fact]
    public void Merge_WithoutName()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseWithoutName(1)), TestData.Culture);
        var sapiCourse = GetApiCourseWithoutName(1);
        courseCi.Merge(new CourseDto(sapiCourse), TestData.CultureNl);

        Assert.NotNull(courseCi);
        Assert.Equal(DefaultCourseId, courseCi.Id);
        Assert.Empty(courseCi.Names);
        ValidateHoles(courseCi.Holes, 1);
    }

    [Fact]
    public void Merge_WithoutHoles()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseWithoutHoles()), TestData.Culture);
        var sapiCourse = GetApiCourseWithoutHoles();
        sapiCourse.name = "New Name";
        courseCi.Merge(new CourseDto(sapiCourse), TestData.CultureNl);

        Assert.NotNull(courseCi);
        Assert.Equal(DefaultCourseId, courseCi.Id);
        Assert.Equal(2, courseCi.Names.Count);
        Assert.Equal(DefaultCourseName, courseCi.Names[TestData.Culture]);
        Assert.Equal(sapiCourse.name, courseCi.Names[TestData.CultureNl]);
        ValidateHoles(courseCi.Holes, 0);
    }

    [Fact]
    public void Full_MergeWithoutId()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseFull()), TestData.Culture);
        var sapiCourse = GetApiCourseWithoutId();
        sapiCourse.name = "New Name";
        courseCi.Merge(new CourseDto(sapiCourse), TestData.CultureNl);

        Assert.NotNull(courseCi);
        Assert.Equal(DefaultCourseId, courseCi.Id);
        Assert.Equal(2, courseCi.Names.Count);
        Assert.Equal(DefaultCourseName, courseCi.Names[TestData.Culture]);
        Assert.Equal(sapiCourse.name, courseCi.Names[TestData.CultureNl]);
        ValidateHoles(courseCi.Holes);
    }

    [Fact]
    public void Full_MergeWithoutName()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseFull()), TestData.Culture);
        var sapiCourse = GetApiCourseWithoutName();
        courseCi.Merge(new CourseDto(sapiCourse), TestData.CultureNl);

        Assert.NotNull(courseCi);
        Assert.Equal(DefaultCourseId, courseCi.Id);
        Assert.Single(courseCi.Names);
        Assert.Equal(DefaultCourseName, courseCi.Names[TestData.Culture]);
        Assert.False(courseCi.Names.ContainsKey(TestData.CultureNl));
        ValidateHoles(courseCi.Holes);
    }

    [Fact]
    public void Full_MergeWithoutHoles()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseFull()), TestData.Culture);
        var sapiCourse = GetApiCourseWithoutHoles();
        courseCi.Merge(new CourseDto(sapiCourse), TestData.CultureNl);

        Assert.NotNull(courseCi);
        Assert.Equal(DefaultCourseId, courseCi.Id);
        Assert.Equal(2, courseCi.Names.Count);
        Assert.Equal(DefaultCourseName, courseCi.Names[TestData.Culture]);
        Assert.Equal(sapiCourse.name, courseCi.Names[TestData.CultureNl]);
        ValidateHoles(courseCi.Holes);
    }

    [Fact]
    public void Full_MergeWith18Holes()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseFull()), TestData.Culture);
        var sapiCourse = GetApiCourseFull(18);
        courseCi.Merge(new CourseDto(sapiCourse), TestData.CultureNl);

        Assert.NotNull(courseCi);
        Assert.Equal(DefaultCourseId, courseCi.Id);
        Assert.Equal(2, courseCi.Names.Count);
        Assert.Equal(DefaultCourseName, courseCi.Names[TestData.Culture]);
        Assert.Equal(sapiCourse.name, courseCi.Names[TestData.CultureNl]);
        ValidateHoles(courseCi.Holes, 18);
    }

    [Fact]
    public void WithoutName_MergeFull()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseWithoutName()), TestData.Culture);
        var sapiCourse = GetApiCourseFull();
        sapiCourse.name = "New Name";
        courseCi.Merge(new CourseDto(sapiCourse), TestData.CultureNl);

        Assert.NotNull(courseCi);
        Assert.Equal(DefaultCourseId, courseCi.Id);
        Assert.Single(courseCi.Names);
        Assert.False(courseCi.Names.ContainsKey(TestData.Culture));
        Assert.Equal(sapiCourse.name, courseCi.Names[TestData.CultureNl]);
        ValidateHoles(courseCi.Holes);
    }

    [Fact]
    public void WithoutName_MergeWithoutId()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseWithoutName()), TestData.Culture);
        var sapiCourse = GetApiCourseWithoutId();
        sapiCourse.name = "New Name";
        courseCi.Merge(new CourseDto(sapiCourse), TestData.CultureNl);

        Assert.NotNull(courseCi);
        Assert.Equal(DefaultCourseId, courseCi.Id);
        Assert.Single(courseCi.Names);
        Assert.False(courseCi.Names.ContainsKey(TestData.Culture));
        Assert.Equal(sapiCourse.name, courseCi.Names[TestData.CultureNl]);
        ValidateHoles(courseCi.Holes);
    }

    [Fact]
    public void WithoutName_MergeWithoutName()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseWithoutName()), TestData.Culture);
        var sapiCourse = GetApiCourseWithoutName();
        courseCi.Merge(new CourseDto(sapiCourse), TestData.CultureNl);

        Assert.NotNull(courseCi);
        Assert.Equal(DefaultCourseId, courseCi.Id);
        Assert.Empty(courseCi.Names);
        ValidateHoles(courseCi.Holes);
    }

    [Fact]
    public void WithoutName_MergeWithoutHoles()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseWithoutName()), TestData.Culture);
        var sapiCourse = GetApiCourseWithoutHoles();
        courseCi.Merge(new CourseDto(sapiCourse), TestData.CultureNl);

        Assert.NotNull(courseCi);
        Assert.Equal(DefaultCourseId, courseCi.Id);
        Assert.Single(courseCi.Names);
        Assert.False(courseCi.Names.ContainsKey(TestData.Culture));
        Assert.Equal(sapiCourse.name, courseCi.Names[TestData.CultureNl]);
        ValidateHoles(courseCi.Holes);
    }

    [Fact]
    public void WithoutName_MergeWith18Holes()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseWithoutName()), TestData.Culture);
        var sapiCourse = GetApiCourseFull(18);
        courseCi.Merge(new CourseDto(sapiCourse), TestData.CultureNl);

        Assert.NotNull(courseCi);
        Assert.Equal(DefaultCourseId, courseCi.Id);
        Assert.Single(courseCi.Names);
        Assert.False(courseCi.Names.ContainsKey(TestData.Culture));
        Assert.Equal(sapiCourse.name, courseCi.Names[TestData.CultureNl]);
        ValidateHoles(courseCi.Holes, 18);
    }

    [Fact]
    public void WithoutHoles_MergeFull()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseWithoutHoles()), TestData.Culture);
        var sapiCourse = GetApiCourseFull();
        sapiCourse.name = "New Name";
        courseCi.Merge(new CourseDto(sapiCourse), TestData.CultureNl);

        Assert.NotNull(courseCi);
        Assert.Equal(DefaultCourseId, courseCi.Id);
        Assert.Equal(2, courseCi.Names.Count);
        Assert.Equal(DefaultCourseName, courseCi.Names[TestData.Culture]);
        Assert.Equal(sapiCourse.name, courseCi.Names[TestData.CultureNl]);
        ValidateHoles(courseCi.Holes);
    }

    [Fact]
    public void WithoutHoles_MergeWithoutId()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseWithoutHoles()), TestData.Culture);
        var sapiCourse = GetApiCourseWithoutId();
        sapiCourse.name = "New Name";
        courseCi.Merge(new CourseDto(sapiCourse), TestData.CultureNl);

        Assert.NotNull(courseCi);
        Assert.Equal(DefaultCourseId, courseCi.Id);
        Assert.Equal(2, courseCi.Names.Count);
        Assert.Equal(DefaultCourseName, courseCi.Names[TestData.Culture]);
        Assert.Equal(sapiCourse.name, courseCi.Names[TestData.CultureNl]);
        ValidateHoles(courseCi.Holes);
    }

    [Fact]
    public void WithoutHoles_MergeWithoutName()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseWithoutHoles()), TestData.Culture);
        var sapiCourse = GetApiCourseWithoutName();
        courseCi.Merge(new CourseDto(sapiCourse), TestData.CultureNl);

        Assert.NotNull(courseCi);
        Assert.Equal(DefaultCourseId, courseCi.Id);
        Assert.Single(courseCi.Names);
        Assert.Equal(DefaultCourseName, courseCi.Names[TestData.Culture]);
        Assert.False(courseCi.Names.ContainsKey(TestData.CultureNl));
        ValidateHoles(courseCi.Holes);
    }

    [Fact]
    public void WithoutHoles_MergeWithoutHoles()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseWithoutHoles()), TestData.Culture);
        var sapiCourse = GetApiCourseWithoutHoles();
        courseCi.Merge(new CourseDto(sapiCourse), TestData.CultureNl);

        Assert.NotNull(courseCi);
        Assert.Equal(DefaultCourseId, courseCi.Id);
        Assert.Equal(2, courseCi.Names.Count);
        Assert.Equal(DefaultCourseName, courseCi.Names[TestData.Culture]);
        Assert.Equal(sapiCourse.name, courseCi.Names[TestData.CultureNl]);
        ValidateHoles(courseCi.Holes, 0);
    }

    [Fact]
    public void WithoutHoles_MergeWith18Holes()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseWithoutHoles()), TestData.Culture);
        var sapiCourse = GetApiCourseFull(18);
        courseCi.Merge(new CourseDto(sapiCourse), TestData.CultureNl);

        Assert.NotNull(courseCi);
        Assert.Equal(DefaultCourseId, courseCi.Id);
        Assert.Equal(2, courseCi.Names.Count);
        Assert.Equal(DefaultCourseName, courseCi.Names[TestData.Culture]);
        Assert.Equal(sapiCourse.name, courseCi.Names[TestData.CultureNl]);
        ValidateHoles(courseCi.Holes, 18);
    }
}
