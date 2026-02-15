using SafetyMap.Core.DTOs.UserSubscription;

namespace SafetyMap.Core.Contracts
{
    public interface IUserSubscriptionService
    {
        Task<IEnumerable<UserSubscriptionDTO>> GetAllAsync();
        Task<UserSubscriptionDTO?> GetByIdAsync(Guid id);
        Task CreateAsync(UserSubscriptionCreateDTO dto);
        Task UpdateAsync(UserSubscriptionEditDTO dto);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<KeyValuePair<string, string>>> GetNeighborhoodSelectListAsync();
    }
}
