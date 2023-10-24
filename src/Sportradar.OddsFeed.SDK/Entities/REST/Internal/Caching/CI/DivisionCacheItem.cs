using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI
{
    internal class DivisionCacheItem
    {
        public int? Id { get; }

        public string Name { get; }

        public DivisionCacheItem(DivisionDto divisionDto)
        {
            Guard.Argument(divisionDto, nameof(divisionDto)).NotNull();

            Id = divisionDto.Id;
            Name = divisionDto.Name;
        }

        public DivisionCacheItem(ExportableDivision exportableDivision)
        {
            Guard.Argument(exportableDivision, nameof(exportableDivision)).NotNull();

            Id = exportableDivision.Id;
            Name = exportableDivision.Name;
        }

        public ExportableDivision Export()
        {
            return new ExportableDivision { Id = Id, Name = Name };
        }
    }
}
