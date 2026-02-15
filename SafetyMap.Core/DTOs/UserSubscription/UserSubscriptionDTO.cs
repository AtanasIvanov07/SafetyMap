namespace SafetyMap.Core.DTOs.UserSubscription
{
    public class UserSubscriptionDTO
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public Guid NeighborhoodId { get; set; }
        public string NeighborhoodName { get; set; } = string.Empty;
        public DateTime SubscribedAt { get; set; }
    }
}
