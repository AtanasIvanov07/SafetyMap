using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafetyMapData.Entities
{
    public class UserIdentity : IdentityUser
     {
        public string  FirstName { get; set; }
        public string LastName { get; set; }
        public virtual ICollection<UserSubscription> Subscriptions { get; set; }

        public UserIdentity()
        {
            Subscriptions = new HashSet<UserSubscription>();
        }


    }
}
