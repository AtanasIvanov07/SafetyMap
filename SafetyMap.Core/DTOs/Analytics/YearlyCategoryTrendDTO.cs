namespace SafetyMap.Core.DTOs.Analytics
{
    public class YearlyCategoryTrendDTO
    {
        public string CategoryName { get; set; } = string.Empty;
        public string ColorCode { get; set; } = string.Empty;
        public List<CrimeTrendPointDTO> DataPoints { get; set; } = new();
    }
}
