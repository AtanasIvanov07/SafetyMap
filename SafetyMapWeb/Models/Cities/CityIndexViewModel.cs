namespace SafetyMapWeb.Models.Cities
{
    public class CityIndexViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int Population { get; set; }
    }
}
