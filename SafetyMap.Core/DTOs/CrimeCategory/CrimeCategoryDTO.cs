namespace SafetyMap.Core.DTOs.CrimeCategory
{
    public class CrimeCategoryDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ColorCode { get; set; } = string.Empty;
    }
}
