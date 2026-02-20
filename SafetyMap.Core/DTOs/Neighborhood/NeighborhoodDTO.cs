namespace SafetyMap.Core.DTOs.Neighborhood
{
    public class NeighborhoodDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int SafetyRating { get; set; }
        public Guid CityId { get; set; }
        public string CityName { get; set; } = string.Empty;
    }
}
