namespace SafetyMap.Core.DTOs.City
{
    public class CityEditDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Population { get; set; }
    }
}
