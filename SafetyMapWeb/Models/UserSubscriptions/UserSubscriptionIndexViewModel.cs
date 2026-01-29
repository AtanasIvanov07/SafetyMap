namespace SafetyMapWeb.Models.UserSubscriptions
{
    public class UserSubscriptionIndexViewModel
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string NeighborhoodName { get; set; } = string.Empty;
        public DateTime SubscribedAt { get; set; }
    }
}
