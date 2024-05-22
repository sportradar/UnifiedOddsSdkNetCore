// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    internal class Course : EntityPrinter, ICourse
    {
        public Urn Id { get; set; }
        public IReadOnlyDictionary<CultureInfo, string> Names { get; set; }
        public ICollection<IHole> Holes { get; set; }

        public Course(CourseCacheItem courseCi, IReadOnlyCollection<CultureInfo> cultures)
        {
            Guard.Argument(courseCi, nameof(courseCi)).NotNull();
            Guard.Argument(cultures, nameof(cultures)).NotNull().NotEmpty();

            Id = courseCi.Id;
            Names = SdkInfo.GetOrCreateReadOnlyNames(courseCi.Names, cultures);
            Holes = new List<IHole>();
            if (!courseCi.Holes.IsNullOrEmpty())
            {
                Holes = new Collection<IHole>(courseCi.Holes.Select(h => (IHole)new Hole(h)).ToList());
            }
        }

        protected override string PrintI()
        {
            return $"Id={Id}";
        }

        protected override string PrintC()
        {
            return $"Id={Id}, Name[{Names.Keys.First().TwoLetterISOLanguageName}]={Names.Values.First()}, Holes={Holes.Count}";
        }

        protected override string PrintF()
        {
            var names = string.Join(", ", Names.Select(x => x.Key.TwoLetterISOLanguageName + ":" + x.Value));

            var holes = string.Empty;
            holes = Holes.IsNullOrEmpty() ? "0" : string.Join(", ", Holes.Select(x => x.Number + "-" + x.Par));

            return $"Id={Id}, Names=[{names}], Holes=[{holes}]";
        }

        protected override string PrintJ()
        {
            return PrintJ(GetType(), this);
        }
    }
}
