namespace SafetyMapWeb.Models.Neighborhoods
{
    public class NeighborhoodIndexViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int SafetyRating { get; set; }
        public string CityName { get; set; } = string.Empty;
    }
}
