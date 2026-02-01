namespace SafetyMapWeb.Models.Cities
{
    public class CityIndexViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public int Population { get; set; }
    }
}
