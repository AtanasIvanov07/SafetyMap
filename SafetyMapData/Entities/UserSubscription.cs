using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafetyMapData.Entities
{
    public class UserSubscription
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public UserIdentity? User { get; set; }
        public int NeighborhoodId { get; set; }
        public Neighborhood? Neighborhood { get; set; }

        public DateTime SubscribedAt { get; set; } = DateTime.Now;
    }
}
