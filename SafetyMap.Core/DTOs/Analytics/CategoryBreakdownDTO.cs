namespace SafetyMap.Core.DTOs.Analytics
{
    public class CategoryBreakdownDTO
    {
        public string CategoryName { get; set; } = string.Empty;
        public string ColorCode { get; set; } = string.Empty;
        public int TotalCrimes { get; set; }
    }
}
