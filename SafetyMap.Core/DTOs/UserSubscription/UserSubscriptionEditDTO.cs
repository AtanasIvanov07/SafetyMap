namespace SafetyMap.Core.DTOs.UserSubscription
{
    public class UserSubscriptionEditDTO
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public Guid NeighborhoodId { get; set; }
        public DateTime SubscribedAt { get; set; }
    }
}
