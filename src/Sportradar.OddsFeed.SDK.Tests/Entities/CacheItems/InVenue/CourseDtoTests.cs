using System;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InVenue;

public class CourseDtoTests : VenueHelper
{
    [Fact]
    public void Hole_Constructor()
    {
        var hole = new hole { number = 10, par = 5 };
        var holeDto = new HoleDto(hole);

        Assert.NotNull(holeDto);
        Assert.Equal(10, holeDto.Number);
        Assert.Equal(5, holeDto.Par);
    }

    [Fact]
    public void Hole_ConstructorWithNumberOnly()
    {
        var hole = new hole { number = 10 };
        var holeDto = new HoleDto(hole);

        Assert.NotNull(holeDto);
        Assert.Equal(10, holeDto.Number);
        Assert.Equal(0, holeDto.Par);
    }

    [Fact]
    public void Hole_ConstructorWithNegativeNumber()
    {
        var hole = new hole { number = -10 };
        var holeDto = new HoleDto(hole);

        Assert.NotNull(holeDto);
        Assert.Equal(-10, holeDto.Number);
        Assert.Equal(0, holeDto.Par);
    }

    [Fact]
    public void Hole_ConstructorWithParOnly()
    {
        var hole = new hole { par = 5 };
        var holeDto = new HoleDto(hole);

        Assert.NotNull(holeDto);
        Assert.Equal(0, holeDto.Number);
        Assert.Equal(5, holeDto.Par);
    }

    [Fact]
    public void Hole_ConstructorWithNegativePar()
    {
        var hole = new hole { par = -5 };
        var holeDto = new HoleDto(hole);

        Assert.NotNull(holeDto);
        Assert.Equal(0, holeDto.Number);
        Assert.Equal(-5, holeDto.Par);
    }

    [Fact]
    public void Hole_ConstructorWithNoData()
    {
        var hole = new hole();
        var holeDto = new HoleDto(hole);

        Assert.NotNull(holeDto);
        Assert.Equal(0, holeDto.Number);
        Assert.Equal(0, holeDto.Par);
    }

    [Fact]
    public void Constructor_Normal()
    {
        var course = new course
        {
            id = DefaultCourseId.ToString(),
            name = DefaultCourseName,
            hole = GetApiHoles(1)
        };

        var courseDto = new CourseDto(course);

        Assert.NotNull(courseDto);
        Assert.Equal(DefaultCourseId, courseDto.Id);
        Assert.Equal(DefaultCourseName, courseDto.Name);
        Assert.Single(courseDto.Holes);
        Assert.Equal(1, courseDto.Holes.First().Number);
        Assert.Equal(4, courseDto.Holes.First().Par);
    }

    [Fact]
    public void Constructor_With9Holes()
    {
        var course = new course
        {
            id = DefaultCourseId.ToString(),
            name = DefaultCourseName,
            hole = GetApiHoles()
        };

        var courseDto = new CourseDto(course);

        Assert.NotNull(courseDto);
        Assert.Equal(DefaultCourseId, courseDto.Id);
        Assert.Equal(DefaultCourseName, courseDto.Name);
        Assert.Equal(9, courseDto.Holes.Count);

        for (var i = 1; i <= 9; i++)
        {
            Assert.Equal(i, courseDto.Holes.ElementAt(i - 1).Number);
            Assert.Equal(4, courseDto.Holes.ElementAt(i - 1).Par);
        }
    }

    [Fact]
    public void Constructor_WithoutId()
    {
        var course = new course
        {
            name = DefaultCourseName,
            hole = GetApiHoles(1)
        };

        var courseDto = new CourseDto(course);

        Assert.NotNull(courseDto);
        Assert.Null(courseDto.Id);
        Assert.Equal(DefaultCourseName, courseDto.Name);
        Assert.Single(courseDto.Holes);
        Assert.Equal(1, courseDto.Holes.First().Number);
        Assert.Equal(4, courseDto.Holes.First().Par);
    }

    [Fact]
    public void Constructor_WrongId_Throws()
    {
        var course = new course
        {
            id = "not-valid-urn"
        };

        var ex = Assert.Throws<FormatException>(() => new CourseDto(course));
        Assert.Contains("Value 'not-valid-urn' is not a valid string", ex.Message);
    }

    [Fact]
    public void Constructor_WithoutName()
    {
        var course = new course
        {
            id = DefaultCourseId.ToString(),
            hole = GetApiHoles(1)
        };

        var courseDto = new CourseDto(course);

        Assert.NotNull(courseDto);
        Assert.Equal(DefaultCourseId, courseDto.Id);
        Assert.Null(courseDto.Name);
        Assert.Single(courseDto.Holes);
        Assert.Equal(1, courseDto.Holes.First().Number);
        Assert.Equal(4, courseDto.Holes.First().Par);
    }

    [Fact]
    public void Constructor_WithoutHoles()
    {
        var course = new course
        {
            id = DefaultCourseId.ToString(),
            name = DefaultCourseName
        };

        var courseDto = new CourseDto(course);

        Assert.NotNull(courseDto);
        Assert.Equal(DefaultCourseId, courseDto.Id);
        Assert.Equal(DefaultCourseName, courseDto.Name);
        Assert.Empty(courseDto.Holes);
    }
}
