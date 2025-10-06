// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InVenue;

public class VenueCiTests : VenueHelper
{
    private readonly CultureInfo _cultureFr = new CultureInfo("fr");
    private readonly CultureInfo _cultureRu = new CultureInfo("ru");

    [Fact]
    public void Constructor_VenueIsNotNull_ReturnsCi()
    {
        var sapiVenue = GenerateApiVenue(courseSize: 1);
        var venueCi = new VenueCacheItem(new VenueDto(sapiVenue), TestData.Culture);

        ValidateVenueCi(sapiVenue, venueCi, TestData.Culture);
    }

    [Fact]
    public void Constructor_NameIsNull_ReturnsCi()
    {
        var sapiVenue = GenerateApiVenue(courseSize: 1);
        sapiVenue.name = null;
        var venueCi = new VenueCacheItem(new VenueDto(sapiVenue), TestData.Culture);

        ValidateVenueCi(sapiVenue, venueCi, TestData.Culture);
    }

    [Fact]
    public void Constructor_VenueIsNull_ThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new VenueCacheItem(null, TestData.Culture));
    }

    [Fact]
    public void Constructor_VenueIdIsNull_ThrowArgumentNullException()
    {
        var sapiVenue = GenerateApiVenue(courseSize: 0);
        sapiVenue.id = null;
        Assert.Throws<ArgumentNullException>(() => new VenueCacheItem(new VenueDto(sapiVenue), TestData.Culture));
    }

    [Fact]
    public void Constructor_ExportableVenueIsNotNull_ReturnCi()
    {
        var exportableVenue = new ExportableVenue() { Id = DefaultVenueId.ToString() };
        var venueCi = new VenueCacheItem(exportableVenue);

        Assert.NotNull(venueCi);
        Assert.Equal(exportableVenue.Id, venueCi.Id.ToString());
    }

    [Fact]
    public void Constructor_ExportableVenueWithCourses_ReturnCi()
    {
        var exportableCourse = new ExportableCourse { Id = DefaultCourseId.ToString(), Holes = new List<ExportableHole> { new() { Number = 2, Par = 4 }, new() { Number = 5, Par = 4 } } };
        var exportableVenue = new ExportableVenue { Id = DefaultVenueId.ToString(), Courses = new List<ExportableCourse> { exportableCourse } };
        var venueCi = new VenueCacheItem(exportableVenue);

        Assert.NotNull(venueCi);
        Assert.Equal(exportableVenue.Id, venueCi.Id.ToString());
        Assert.Single(venueCi.Courses);
        Assert.Equal(2, venueCi.Courses.First().Holes.Count);
    }

    [Fact]
    public void Constructor_ExportableVenueIsNull_ThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new VenueCacheItem(null));
    }

    [Fact]
    public void Constructor_ExportableVenueWithoutId_ThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new VenueCacheItem(new ExportableVenue()));
    }

    [Fact]
    public void Merge_VenueIsNull_ThrowArgumentNullException()
    {
        var venueCi = new VenueCacheItem(new VenueDto(GenerateApiVenue(courseSize: 1)), TestData.CultureNl);
        Assert.NotNull(venueCi);

        Assert.Throws<ArgumentNullException>(() => venueCi.Merge(null, TestData.Culture));
    }

    [Fact]
    public void GetName_HasNoValue_ReturnNull()
    {
        var sapiVenue = GenerateApiVenue(courseSize: 1);
        var venueCi = new VenueCacheItem(new VenueDto(sapiVenue), TestData.Culture);

        var result = venueCi.GetName(TestData.CultureNl);

        Assert.Null(result);
    }

    [Fact]
    public void GetCity_HasNoValue_ReturnNull()
    {
        var sapiVenue = GenerateApiVenue(courseSize: 1);
        var venueCi = new VenueCacheItem(new VenueDto(sapiVenue), TestData.Culture);

        var result = venueCi.GetCity(TestData.CultureNl);

        Assert.Null(result);
    }

    [Fact]
    public void GetCountry_HasNoValue_ReturnNull()
    {
        var sapiVenue = GenerateApiVenue(courseSize: 1);
        var venueCi = new VenueCacheItem(new VenueDto(sapiVenue), TestData.Culture);

        var result = venueCi.GetCountry(TestData.CultureNl);

        Assert.Null(result);
    }

    [Fact]
    public async Task ExportAsync_WhenCalled_ReturnExportableVenue()
    {
        var sapiVenue = GenerateApiVenue(courseSize: 1);
        var venueCi = new VenueCacheItem(new VenueDto(sapiVenue), TestData.Culture);

        var exportableVenue = await venueCi.ExportAsync();

        Assert.NotNull(exportableVenue);
        Assert.Equal(Urn.Parse(exportableVenue.Id), venueCi.Id);
        Assert.Single(exportableVenue.Names);
        Assert.Equal(exportableVenue.Names[TestData.Culture], venueCi.GetName(TestData.Culture));
        Assert.Equal(exportableVenue.CityNames[TestData.Culture], venueCi.GetCity(TestData.Culture));
        Assert.Equal(exportableVenue.CountryNames[TestData.Culture], venueCi.GetCountry(TestData.Culture));
        Assert.Equal(exportableVenue.CountryCode, venueCi.CountryCode);
        Assert.Equal(exportableVenue.State, venueCi.State);
        Assert.Equal(exportableVenue.Capacity, venueCi.Capacity);
        Assert.Equal(exportableVenue.Courses.Count, venueCi.Courses.Count);

        foreach (var course in venueCi.Courses)
        {
            var courseCi = exportableVenue.Courses.First(c => c.Id.Equals(course.Id.ToString()));
            Assert.NotNull(courseCi);
            Assert.Equal(course.Names[TestData.Culture], courseCi.Names[TestData.Culture]);
            Assert.Equal(course.Holes.Count, courseCi.Holes.Count);
        }
    }

    [Fact]
    public void MergeCourses_EmptyCollection_ReturnEmpty()
    {
        var sapiVenue = GenerateApiVenue(courseSize: 0);
        var venueCi = new VenueCacheItem(new VenueDto(sapiVenue), TestData.Culture);

        Assert.NotNull(venueCi);
        Assert.NotNull(venueCi.Courses);
        Assert.Empty(venueCi.Courses);
    }

    [Fact]
    public void MergeCourses_OneCourse_ReturnOne()
    {
        var sapiVenue = GenerateApiVenue(courseSize: 1);
        var venueCi = new VenueCacheItem(new VenueDto(sapiVenue), TestData.Culture);

        Assert.NotNull(venueCi);
        Assert.NotNull(venueCi.Courses);
        Assert.Single(venueCi.Courses);
        Assert.Equal(9, venueCi.Courses.First().Holes.Count);
    }

    [Fact]
    public void MergeCourses_NonePreexisting_ReturnNew()
    {
        var sapiVenue = GenerateApiVenue(courseSize: 0);
        var venueCi = new VenueCacheItem(new VenueDto(sapiVenue), TestData.Culture);
        var sapiVenue2 = GenerateApiVenue(courseSize: 1);
        venueCi.Merge(new VenueDto(sapiVenue2), TestData.CultureNl);

        Assert.NotNull(venueCi);
        Assert.NotNull(venueCi.Courses);
        Assert.Single(venueCi.Courses);
        ValidateVenueCi(sapiVenue2, venueCi, TestData.CultureNl);
    }

    [Fact]
    public void MergeCourses_WithOneExisting_IsMerged()
    {
        var sapiVenue = GenerateApiVenue(courseSize: 1);
        var venueCi = new VenueCacheItem(new VenueDto(sapiVenue), TestData.Culture);
        var sapiVenue2 = GenerateApiVenue(courseSize: 1);
        venueCi.Merge(new VenueDto(sapiVenue2), TestData.CultureNl);

        Assert.NotNull(venueCi);
        Assert.NotNull(venueCi.Courses);
        Assert.Single(venueCi.Courses);
        ValidateVenueCi(sapiVenue2, venueCi, TestData.CultureNl);
    }

    [Fact]
    public void MergeCourses_WithOneExistingWith9Holes_IsReplaced()
    {
        var sapiVenue = GenerateApiVenue(courseSize: 1);
        var venueCi = new VenueCacheItem(new VenueDto(sapiVenue), TestData.Culture);
        var sapiVenue2 = GenerateApiVenue(courseSize: 1);
        sapiVenue2.course[0].hole = GetApiHoles(18);
        venueCi.Merge(new VenueDto(sapiVenue2), TestData.CultureNl);

        Assert.NotNull(venueCi);
        Assert.NotNull(venueCi.Courses);
        Assert.Single(venueCi.Courses);
        Assert.Equal(18, venueCi.Courses.First().Holes.Count);
        ValidateVenueCi(sapiVenue2, venueCi, TestData.CultureNl);
    }

    [Fact]
    public void MergeCourses_WithOneExisting_AddNewCourses()
    {
        var sapiVenue = GenerateApiVenue(courseSize: 1);
        var venueCi = new VenueCacheItem(new VenueDto(sapiVenue), TestData.Culture);
        var sapiVenue2 = GenerateApiVenue(courseSize: 3);
        sapiVenue2.course[0].hole = GetApiHoles(18);
        venueCi.Merge(new VenueDto(sapiVenue2), TestData.CultureNl);

        Assert.NotNull(venueCi);
        Assert.NotNull(venueCi.Courses);
        Assert.Equal(3, venueCi.Courses.Count);
        Assert.Equal(18, venueCi.Courses.First().Holes.Count);
        ValidateVenueCi(sapiVenue2, venueCi, TestData.CultureNl);
    }

    [Fact]
    public void MergeCourses_WithExisting_AreReplaced()
    {
        var sapiVenue = GenerateApiVenue(courseSize: 3);
        var venueCi = new VenueCacheItem(new VenueDto(sapiVenue), TestData.Culture);
        var sapiVenue2 = GenerateApiVenue(courseSize: 1);
        venueCi.Merge(new VenueDto(sapiVenue2), TestData.CultureNl);

        Assert.NotNull(venueCi);
        Assert.NotNull(venueCi.Courses);
        Assert.Single(venueCi.Courses);
        ValidateVenueCi(sapiVenue2, venueCi, TestData.CultureNl);
    }

    [Fact]
    public void HasTranslationFor_NameIsNull()
    {
        var sapiVenue = GenerateApiVenue(courseSize: 1);
        sapiVenue.name = null;
        var venueCi = new VenueCacheItem(new VenueDto(sapiVenue), TestData.Culture);

        Assert.True(venueCi.HasTranslationsFor(new List<CultureInfo> { TestData.Culture }));
    }

    [Fact]
    public void HasTranslationFor_RequestForNewCulture_ReturnFalse()
    {
        var sapiVenue = GenerateApiVenue(courseSize: 1);
        var venueCi = new VenueCacheItem(new VenueDto(sapiVenue), TestData.Culture);

        Assert.False(venueCi.HasTranslationsFor(new List<CultureInfo> { TestData.CultureNl }));
    }

    [Fact]
    public void HasTranslationFor_Merge2Names_ReturnTrue()
    {
        var sapiVenue = GenerateApiVenue(courseSize: 1);
        var venueCi = new VenueCacheItem(new VenueDto(sapiVenue), TestData.Culture);
        var sapiVenue2 = GenerateApiVenue(courseSize: 1);
        sapiVenue2.name = "Second name";
        venueCi.Merge(new VenueDto(sapiVenue2), TestData.CultureNl);

        Assert.True(venueCi.HasTranslationsFor(new List<CultureInfo> { TestData.Culture, TestData.CultureNl }));
        Assert.False(venueCi.GetName(TestData.Culture).IsNullOrEmpty());
        Assert.False(venueCi.GetName(TestData.CultureNl).IsNullOrEmpty());
        Assert.NotEqual(venueCi.GetName(TestData.Culture), venueCi.GetName(TestData.CultureNl));
    }

    /// <summary>
    /// Given 3 course in a cache item with ids 1,2,3 and translations in en, en, en
    /// When I receive inflight request with 3 courses with ids 2,5,7 and translations en,en,en
    /// Then cache contains item 3 courses with ids 2,5,7 and translations en,en,en
    /// </summary>
    [Fact]
    public void ThreeCourses_MergedWith3CoursesWithDifferentIds()
    {
        var sapiVenue1 = GenerateApiVenue(TestData.Culture, courseSize: 3);
        var sapiVenue2 = GenerateApiVenue(TestData.Culture, courseSize: 3);
        sapiVenue2.course[0].id = "sr:venue:2";
        sapiVenue2.course[1].id = "sr:venue:5";
        sapiVenue2.course[2].id = "sr:venue:7";

        var venueCi = new VenueCacheItem(new VenueDto(sapiVenue1), TestData.Culture);
        venueCi.Merge(new VenueDto(sapiVenue2), TestData.Culture);

        Assert.Equal(3, venueCi.Courses.Count);
        Assert.Equal(Urn.Parse("sr:venue:2"), venueCi.Courses.First().Id);
        Assert.Equal(Urn.Parse("sr:venue:5"), venueCi.Courses.Skip(1).First().Id);
        Assert.Equal(Urn.Parse("sr:venue:7"), venueCi.Courses.Skip(2).First().Id);
        Assert.Single(venueCi.Courses.First().Names);
        Assert.Single(venueCi.Courses.Skip(1).First().Names);
        Assert.Single(venueCi.Courses.Skip(2).First().Names);
        Assert.Equal(TestData.Culture, venueCi.Courses.First().Names.Keys.First());
        Assert.Equal(TestData.Culture, venueCi.Courses.Skip(1).First().Names.Keys.First());
        Assert.Equal(TestData.Culture, venueCi.Courses.Skip(2).First().Names.Keys.First());
    }

    /// <summary>
    /// Given 3 course in a cache item with ids 1,2,3 and translations in fr, fr, fr
    /// When I receive inflight request with 3 courses with ids 2,5,7 and translations en,en,en
    /// Then cache contains item 3 courses with ids 2,5,7 and translations en/fr,en,en
    /// </summary>
    [Fact]
    public void ThreeCourses_MergedWith3CoursesWithDifferentIdsAndDifferentCulture()
    {
        var sapiVenue1 = GenerateApiVenue(_cultureFr, courseSize: 3);
        var sapiVenue2 = GenerateApiVenue(TestData.Culture, courseSize: 3);
        sapiVenue2.course[0].id = "sr:venue:2";
        sapiVenue2.course[1].id = "sr:venue:5";
        sapiVenue2.course[2].id = "sr:venue:7";

        var venueCi = new VenueCacheItem(new VenueDto(sapiVenue1), _cultureFr);
        venueCi.Merge(new VenueDto(sapiVenue2), TestData.Culture);

        Assert.Equal(3, venueCi.Courses.Count);
        Assert.Equal(Urn.Parse("sr:venue:2"), venueCi.Courses.First().Id);
        Assert.Equal(Urn.Parse("sr:venue:5"), venueCi.Courses.Skip(1).First().Id);
        Assert.Equal(Urn.Parse("sr:venue:7"), venueCi.Courses.Skip(2).First().Id);
        Assert.Equal(2, venueCi.Courses.First().Names.Count);
        Assert.Single(venueCi.Courses.Skip(1).First().Names);
        Assert.Single(venueCi.Courses.Skip(2).First().Names);
        Assert.Contains(TestData.Culture, venueCi.Courses.First().Names.Keys);
        Assert.Contains(_cultureFr, venueCi.Courses.First().Names.Keys);
        Assert.Equal(TestData.Culture, venueCi.Courses.Skip(1).First().Names.Keys.First());
        Assert.Equal(TestData.Culture, venueCi.Courses.Skip(2).First().Names.Keys.First());
    }

    /// <summary>
    /// Given 3 course in a cache item with ids 1,2,3 and translations in fr, fr, fr
    /// When I receive inflight request with 1 courses with ids 2 and translations en
    /// Then cache contains item 1 courses with ids 2 and translations en/fr
    /// </summary>
    [Fact]
    public void ThreeCourses_MergedWithOneCourseWithExistingCourseIdAndDifferentCulture()
    {
        var sapiVenue1 = GenerateApiVenue(_cultureFr, courseSize: 3);
        var sapiVenue2 = GenerateApiVenue(TestData.Culture, courseSize: 1);
        sapiVenue2.course[0].id = "sr:venue:2";

        var venueCi = new VenueCacheItem(new VenueDto(sapiVenue1), _cultureFr);
        venueCi.Merge(new VenueDto(sapiVenue2), TestData.Culture);

        Assert.Single(venueCi.Courses);
        Assert.Equal(Urn.Parse("sr:venue:2"), venueCi.Courses.First().Id);
        Assert.Equal(2, venueCi.Courses.First().Names.Count);
        Assert.Contains(TestData.Culture, venueCi.Courses.First().Names.Keys);
        Assert.Contains(_cultureFr, venueCi.Courses.First().Names.Keys);
        Assert.NotEqual(venueCi.Courses.First().Names[TestData.Culture], venueCi.Courses.First().Names[_cultureFr]);
    }

    /// <summary>
    /// Given cache contains item 1 courses with ids 1 and translations en/fr
    /// When I receive inflight request with 2 courses with ids 1,2 and translations ru,ru
    /// Then cache contains item 2 courses with ids 1,2 and translations en/fr/ru,ru
    /// </summary>
    [Fact]
    public void CourseWith2Translations_MergedWith2CoursesWithOneSameIdAndDifferentCulture()
    {
        var sapiVenue1 = GenerateApiVenue(_cultureFr, courseSize: 1);
        var sapiVenue2 = GenerateApiVenue(TestData.Culture, courseSize: 1);
        var sapiVenue3 = GenerateApiVenue(_cultureRu, courseSize: 2);

        var venueCi = new VenueCacheItem(new VenueDto(sapiVenue1), _cultureFr);
        venueCi.Merge(new VenueDto(sapiVenue2), TestData.Culture);
        venueCi.Merge(new VenueDto(sapiVenue3), _cultureRu);

        Assert.Equal(2, venueCi.Courses.Count);
        Assert.Equal(Urn.Parse("sr:venue:1"), venueCi.Courses.First().Id);
        Assert.Equal(Urn.Parse("sr:venue:2"), venueCi.Courses.Skip(1).First().Id);
        Assert.Equal(3, venueCi.Courses.First().Names.Count);
        Assert.Single(venueCi.Courses.Skip(1).First().Names);
        Assert.Contains(TestData.Culture, venueCi.Courses.First().Names.Keys);
        Assert.Contains(_cultureFr, venueCi.Courses.First().Names.Keys);
        Assert.Contains(_cultureRu, venueCi.Courses.First().Names.Keys);
        Assert.Contains(_cultureRu, venueCi.Courses.Skip(1).First().Names.Keys);
        Assert.NotEqual(venueCi.Courses.First().Names[TestData.Culture], venueCi.Courses.First().Names[_cultureFr]);
        Assert.NotEqual(venueCi.Courses.First().Names[TestData.Culture], venueCi.Courses.First().Names[_cultureRu]);
        Assert.NotEqual(venueCi.Courses.First().Names[_cultureRu], venueCi.Courses.First().Names[_cultureFr]);
    }

    /// <summary>
    /// Given 3 course in a cache item with ids 1,2,3 and translations in en, en, en
    /// When I receive inflight request with 3 courses with ids null,2,7 and translations en,en,en
    /// Then cache contains item 3 courses with ids null,2,7 and translations en,en,en
    /// </summary>
    [Fact]
    public void ThreeCoursesWithNullId_MergedWith3CoursesWithOneSameNullIdForSameLanguage()
    {
        var sapiVenue1 = GenerateApiVenue(TestData.Culture, courseSize: 3);
        sapiVenue1.course[0].id = null;
        var sapiVenue2 = GenerateApiVenue(TestData.Culture, courseSize: 3);
        sapiVenue2.course[0].id = null;
        sapiVenue2.course[2].id = "sr:venue:7";

        var venueCi = new VenueCacheItem(new VenueDto(sapiVenue1), TestData.Culture);
        venueCi.Merge(new VenueDto(sapiVenue2), TestData.Culture);

        Assert.Equal(3, venueCi.Courses.Count);
        Assert.Null(venueCi.Courses.First().Id);
        Assert.Equal(Urn.Parse("sr:venue:2"), venueCi.Courses.Skip(1).First().Id);
        Assert.Equal(Urn.Parse("sr:venue:7"), venueCi.Courses.Skip(2).First().Id);
        Assert.Single(venueCi.Courses.First().Names);
        Assert.Single(venueCi.Courses.Skip(1).First().Names);
        Assert.Single(venueCi.Courses.Skip(2).First().Names);
        Assert.Contains(TestData.Culture, venueCi.Courses.First().Names.Keys);
        Assert.Contains(TestData.Culture, venueCi.Courses.First().Names.Keys);
        Assert.Contains(TestData.Culture, venueCi.Courses.Skip(1).First().Names.Keys);
    }

    /// <summary>
    /// Given 3 courses in a cache item with ids 1,2,3 and translations in fr, fr, fr
    /// When I receive inflight request with 1 courses with ids 2 and translations en
    /// Then cache contains item 1 courses with ids 2 and translations en/fr
    /// When I receive another inflight request with 1 course with ids 3 and translations ru
    /// Then cache contains item 1 courses with ids 3 and translations ru
    /// And mismatch happened is logged
    /// </summary>
    [Fact]
    public void ThreeCoursesWith_MergedWithNewCourseForDifferentCulture()
    {
        var sapiVenue1 = GenerateApiVenue(_cultureFr, courseSize: 3);
        var sapiVenue2 = GenerateApiVenue(TestData.Culture, courseSize: 1);
        sapiVenue2.course[0].id = "sr:venue:2";
        var sapiVenue3 = GenerateApiVenue(_cultureRu, courseSize: 1);
        sapiVenue3.course[0].id = "sr:venue:3";

        var venueCi = new VenueCacheItem(new VenueDto(sapiVenue1), _cultureFr);
        venueCi.Merge(new VenueDto(sapiVenue2), TestData.Culture);
        venueCi.Merge(new VenueDto(sapiVenue3), _cultureRu);

        Assert.Single(venueCi.Courses);
        Assert.Equal(Urn.Parse("sr:venue:3"), venueCi.Courses.First().Id);
        Assert.Single(venueCi.Courses.First().Names);
        Assert.Contains(_cultureRu, venueCi.Courses.First().Names.Keys);
    }

    /// <summary>
    /// Given 3 course in a cache item with ids 1,2,3 and translations in en[wim], en[lon], en[endin]
    /// When I receive inflight request with 3 courses with ids null,2,null and translations en[ber],en[lon],en[par]
    /// Then cache contains item 3 courses with ids null,2,null and translations en[ber],en[lon],en[par]
    /// When I receive inflight request with 3 courses with ids 2,null,null and translations fr[lon],fr[vil],fr[riga]
    /// Then cache contains item 3 courses with ids 2,null,null and translations en/fr[lon],en/fr[ber / vil],en/fr[ber / riga]
    /// </summary>
    [Fact]
    public void ThreeCourseWith2NullIds_MergedWithCoursesWith2NullIdsAndDifferentCulture()
    {
        var sapiVenue1 = GenerateApiVenue(TestData.Culture, courseSize: 3);
        sapiVenue1.course[1].name = "lon";
        var sapiVenue2 = GenerateApiVenue(TestData.Culture, courseSize: 3);
        sapiVenue2.course[0].id = null;
        sapiVenue2.course[2].id = null;
        sapiVenue2.course[0].name = "ber";
        sapiVenue2.course[1].name = "lon";
        sapiVenue2.course[2].name = "par";
        var sapiVenue3 = GenerateApiVenue(_cultureFr, courseSize: 3);
        sapiVenue3.course[0].id = "sr:venue:2";
        sapiVenue3.course[1].id = null;
        sapiVenue3.course[2].id = null;
        sapiVenue3.course[0].name = "lon";
        sapiVenue3.course[1].name = "vil";
        sapiVenue3.course[2].name = "riga";

        var venueCi = new VenueCacheItem(new VenueDto(sapiVenue1), TestData.Culture);
        venueCi.Merge(new VenueDto(sapiVenue2), TestData.Culture);
        venueCi.Merge(new VenueDto(sapiVenue3), _cultureFr);

        Assert.Equal(3, venueCi.Courses.Count);
        Assert.Equal(Urn.Parse("sr:venue:2"), venueCi.Courses.First().Id);
        Assert.Null(venueCi.Courses.Skip(1).First().Id);
        Assert.Null(venueCi.Courses.Skip(2).First().Id);
        Assert.Equal(2, venueCi.Courses.First().Names.Count);
        Assert.Equal(2, venueCi.Courses.Skip(1).First().Names.Count);
        Assert.Equal(2, venueCi.Courses.Skip(2).First().Names.Count);
        Assert.Equal(sapiVenue2.course[1].name, venueCi.Courses.First().Names[TestData.Culture]);
        Assert.Equal(sapiVenue3.course[0].name, venueCi.Courses.First().Names[_cultureFr]);
        Assert.Equal(sapiVenue2.course[0].name, venueCi.Courses.Skip(1).First().Names[TestData.Culture]);
        Assert.Equal(sapiVenue3.course[2].name, venueCi.Courses.Skip(1).First().Names[_cultureFr]);
        Assert.Equal(sapiVenue2.course[0].name, venueCi.Courses.Skip(2).First().Names[TestData.Culture]);
        Assert.Equal(sapiVenue3.course[2].name, venueCi.Courses.Skip(2).First().Names[_cultureFr]);
    }
}
