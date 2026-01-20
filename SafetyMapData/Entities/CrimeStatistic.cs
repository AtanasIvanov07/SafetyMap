using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafetyMapData.Entities
{
    public class CrimeStatistic
    {
        public Guid Id { get; set; }

        public Guid NeighborhoodId { get; set; }
        public virtual Neighborhood Neighborhood { get; set; }

        public Guid CrimeCategoryId { get; set; }
        public virtual CrimeCategory CrimeCategory { get; set; }

   
        public int CountOfCrimes { get; set; } 
        public int Year { get; set; } 

      
        public double TrendPercentage { get; set; } 
    }
}
