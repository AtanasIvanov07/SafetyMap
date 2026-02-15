using Microsoft.EntityFrameworkCore;
using SafetyMap.Core.Contracts;
using SafetyMap.Core.DTOs.UserSubscription;
using SafetyMapData;
using SafetyMapData.Entities;

namespace SafetyMap.Core.Services
{
    public class UserSubscriptionService : IUserSubscriptionService
    {
        private readonly SafetyMapDbContext _context;

        public UserSubscriptionService(SafetyMapDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserSubscriptionDTO>> GetAllAsync()
        {
            return await _context.UserSubscriptions
                .Include(u => u.Neighborhood)
                .Select(u => new UserSubscriptionDTO
                {
                    Id = u.Id,
                    UserId = u.UserId,
                    NeighborhoodId = u.NeighborhoodId,
                    NeighborhoodName = u.Neighborhood != null ? u.Neighborhood.Name : "N/A",
                    SubscribedAt = u.SubscribedAt
                })
                .ToListAsync();
        }

        public async Task<UserSubscriptionDTO?> GetByIdAsync(Guid id)
        {
            return await _context.UserSubscriptions
                .Include(u => u.Neighborhood)
                .Where(u => u.Id == id)
                .Select(u => new UserSubscriptionDTO
                {
                    Id = u.Id,
                    UserId = u.UserId,
                    NeighborhoodId = u.NeighborhoodId,
                    NeighborhoodName = u.Neighborhood != null ? u.Neighborhood.Name : "N/A",
                    SubscribedAt = u.SubscribedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task CreateAsync(UserSubscriptionCreateDTO dto)
        {
            var subscription = new UserSubscription
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                NeighborhoodId = dto.NeighborhoodId,
                SubscribedAt = DateTime.Now
            };

            _context.UserSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserSubscriptionEditDTO dto)
        {
            var subscription = await _context.UserSubscriptions.FindAsync(dto.Id);

            if (subscription == null)
            {
                throw new ArgumentException($"UserSubscription with Id '{dto.Id}' was not found.");
            }

            subscription.UserId = dto.UserId;
            subscription.NeighborhoodId = dto.NeighborhoodId;
            subscription.SubscribedAt = dto.SubscribedAt;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var subscription = await _context.UserSubscriptions.FindAsync(id);

            if (subscription != null)
            {
                _context.UserSubscriptions.Remove(subscription);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<KeyValuePair<string, string>>> GetNeighborhoodSelectListAsync()
        {
            return await _context.Neighborhoods
                .Select(n => new KeyValuePair<string, string>(n.Id.ToString(), n.Name))
                .ToListAsync();
        }
    }
}
