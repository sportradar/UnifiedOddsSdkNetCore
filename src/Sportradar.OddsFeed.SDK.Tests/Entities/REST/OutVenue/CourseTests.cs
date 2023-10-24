using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InVenue;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.OutVenue;

public class CourseTests : VenueHelper
{
    [Fact]
    public void Constructor_CourseCacheItemIsNull_ThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Course(null, new List<CultureInfo>()));
    }

    [Fact]
    public void Constructor_CulturesIsNull_ThrowArgumentNullException()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseFull()), TestData.Culture);
        Assert.Throws<ArgumentNullException>(() => new Course(courseCi, null));
    }

    [Fact]
    public void Constructor_CulturesIsEmpty_ThrowArgumentException()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseFull()), TestData.Culture);
        Assert.Throws<ArgumentException>(() => new Course(courseCi, new List<CultureInfo>()));
    }

    [Fact]
    public void Constructor_FullCourseCacheItem_SetPropertiesCorrectly()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseFull()), TestData.Culture);
        var sapiCourse = GetApiCourseFull();
        sapiCourse.name = "Second Name";
        courseCi.Merge(new CourseDto(sapiCourse), TestData.CultureNl);

        var course = new Course(courseCi, new List<CultureInfo> { TestData.Culture, TestData.CultureNl });

        Assert.Equal(courseCi.Id, course.Id);
        Assert.Equal(courseCi.Names[TestData.Culture], course.Names[TestData.Culture]);
        Assert.Equal(courseCi.Names[TestData.CultureNl], course.Names[TestData.CultureNl]);
        Assert.Equal(9, course.Holes.Count);
        Assert.Equal(courseCi.Holes.Count, course.Holes.Count);
    }

    [Fact]
    public void Constructor_CourseCacheItemWithoutHoles_SetPropertiesCorrectly()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseFull(0)), TestData.Culture);
        var sapiCourse = GetApiCourseFull(0);
        sapiCourse.name = "Second Name";
        courseCi.Merge(new CourseDto(sapiCourse), TestData.CultureNl);

        var course = new Course(courseCi, new List<CultureInfo> { TestData.Culture, TestData.CultureNl });

        Assert.Equal(courseCi.Id, course.Id);
        Assert.Equal(courseCi.Names[TestData.Culture], course.Names[TestData.Culture]);
        Assert.Equal(courseCi.Names[TestData.CultureNl], course.Names[TestData.CultureNl]);
        Assert.Empty(course.Holes);
    }

    [Fact]
    public void Constructor_CourseCacheItemWithoutId_SetPropertiesCorrectly()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseWithoutId(0)), TestData.Culture);
        var sapiCourse = GetApiCourseWithoutId(0);
        sapiCourse.name = "Second Name";
        courseCi.Merge(new CourseDto(sapiCourse), TestData.CultureNl);

        var course = new Course(courseCi, new List<CultureInfo> { TestData.Culture, TestData.CultureNl });

        Assert.Null(course.Id);
        Assert.Equal(courseCi.Names[TestData.Culture], course.Names[TestData.Culture]);
        Assert.Equal(courseCi.Names[TestData.CultureNl], course.Names[TestData.CultureNl]);
        Assert.Empty(course.Holes);
    }

    [Fact]
    public void Constructor_CourseCacheItemWithoutName_SetPropertiesCorrectly()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseWithoutName(0)), TestData.Culture);
        var sapiCourse = GetApiCourseWithoutName(0);
        courseCi.Merge(new CourseDto(sapiCourse), TestData.CultureNl);

        var course = new Course(courseCi, new List<CultureInfo> { TestData.Culture, TestData.CultureNl });

        Assert.NotNull(course.Id);
        Assert.Empty(courseCi.Names);
        Assert.True(course.Names.ContainsKey(TestData.Culture));
        Assert.True(course.Names.ContainsKey(TestData.CultureNl));
        Assert.Equal(string.Empty, course.Names[TestData.Culture]);
        Assert.Equal(string.Empty, course.Names[TestData.CultureNl]);
        Assert.Empty(course.Holes);
    }

    [Fact]
    public void PrintI_ValidCourse_ReturnsCorrectString()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseFull()), TestData.Culture);
        var course = new Course(courseCi, new List<CultureInfo> { TestData.Culture });

        var result = course.ToString("I");

        Assert.Equal($"Id={courseCi.Id}", result);
    }

    [Fact]
    public void PrintC_ValidCourse_ReturnsCorrectString()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseFull()), TestData.Culture);
        var course = new Course(courseCi, new List<CultureInfo> { TestData.Culture });

        var result = course.ToString("C");

        Assert.Contains($"Id={courseCi.Id},", result);
        Assert.Contains($"Name[{course.Names.Keys.First().TwoLetterISOLanguageName}]={course.Names.Values.First()},", result);
        Assert.Contains($"Holes={course.Holes.Count}", result);
    }

    [Fact]
    public void PrintC_CourseWithoutId_ReturnsCorrectString()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseWithoutId()), TestData.Culture);
        var course = new Course(courseCi, new List<CultureInfo> { TestData.Culture });

        var result = course.ToString("C");

        Assert.Contains("Id=,", result);
        Assert.Contains($"Name[{course.Names.Keys.First().TwoLetterISOLanguageName}]={course.Names.Values.First()},", result);
        Assert.Contains($"Holes={course.Holes.Count}", result);
    }

    [Fact]
    public void PrintC_CourseWithoutNameWithoutHoles_ReturnsCorrectString()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseWithoutName(0)), TestData.Culture);
        var course = new Course(courseCi, new List<CultureInfo> { TestData.Culture });

        var result = course.ToString("C");

        Assert.Contains($"Id={courseCi.Id},", result);
        Assert.Contains($"Name[{course.Names.Keys.First().TwoLetterISOLanguageName}]=,", result);
        Assert.Contains("Holes=0", result);
    }

    [Fact]
    public void PrintF_ValidCourse_ReturnsCorrectString()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseFull()), TestData.Culture);
        var course = new Course(courseCi, new List<CultureInfo> { TestData.Culture });

        var result = course.ToString("F");

        Assert.Contains($"Id={courseCi.Id},", result);
        Assert.Contains("Names=[", result);
        Assert.Contains("Holes=[", result);
    }

    [Fact]
    public void PrintF_CourseWithoutId_ReturnsCorrectString()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseWithoutId()), TestData.Culture);
        var course = new Course(courseCi, new List<CultureInfo> { TestData.Culture });

        var result = course.ToString("F");

        Assert.Contains("Id=,", result);
        Assert.Contains("Names=[", result);
        Assert.Contains("Holes=[", result);
    }

    [Fact]
    public void PrintF_CourseWithoutNameWithoutHoles_ReturnsCorrectString()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseWithoutName(0)), TestData.Culture);
        var course = new Course(courseCi, new List<CultureInfo> { TestData.Culture });

        var result = course.ToString("F");

        Assert.Contains($"Id={courseCi.Id},", result);
        Assert.Contains($"Names=[{TestData.Culture.TwoLetterISOLanguageName}:],", result);
        Assert.Contains("Holes=[0]", result);
    }

    [Fact]
    public void PrintJ_ValidCourse_ReturnsCorrectString()
    {
        var courseCi = new CourseCacheItem(new CourseDto(GetApiCourseFull()), TestData.Culture);
        var course = new Course(courseCi, new List<CultureInfo> { TestData.Culture });

        Assert.Throws<InvalidDataContractException>(() => course.ToString("J"));
    }
}
