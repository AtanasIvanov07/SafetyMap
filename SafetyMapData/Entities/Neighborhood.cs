using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafetyMapData.Entities
{
    public class Neighborhood
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

   
        public int SafetyRating { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

      
        public Guid CityId { get; set; }

   
        public City? City { get; set; }

     
        public ICollection<CrimeStatistic> CrimeStatistics { get; set; } = new List<CrimeStatistic>();
        public ICollection<UserSubscription> Subscribers { get; set; } = new List<UserSubscription>();
    }
}
