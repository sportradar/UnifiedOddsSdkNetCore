namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    internal class DivisionDto
    {
        public int? Id { get; }

        public string Name { get; }

        internal DivisionDto(int? divisionId, string name)
        {
            Id = divisionId;
            Name = name;
        }
    }
}
