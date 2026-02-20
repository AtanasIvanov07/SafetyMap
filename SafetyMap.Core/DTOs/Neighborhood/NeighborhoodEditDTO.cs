namespace SafetyMap.Core.DTOs.Neighborhood
{
    public class NeighborhoodEditDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int SafetyRating { get; set; }
        public Guid CityId { get; set; }
    }
}
