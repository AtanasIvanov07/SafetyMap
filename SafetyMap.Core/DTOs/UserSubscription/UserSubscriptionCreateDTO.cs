namespace SafetyMap.Core.DTOs.UserSubscription
{
    public class UserSubscriptionCreateDTO
    {
        public string UserId { get; set; } = string.Empty;
        public Guid NeighborhoodId { get; set; }
    }
}
