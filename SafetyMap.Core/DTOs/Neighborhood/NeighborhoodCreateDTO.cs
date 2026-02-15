namespace SafetyMap.Core.DTOs.Neighborhood
{
    public class NeighborhoodCreateDTO
    {
        public string Name { get; set; } = string.Empty;
        public int SafetyRating { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public Guid CityId { get; set; }
    }
}
