using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafetyMapData.Entities
{
    public class CrimeStatistic
    {
        public int Id { get; set; }

        public int NeighborhoodId { get; set; }
        public virtual Neighborhood Neighborhood { get; set; }

        public int CrimeCategoryId { get; set; }
        public virtual CrimeCategory CrimeCategory { get; set; }

   
        public int CountOfCrimes { get; set; } 
        public int Year { get; set; } 

      
        public double TrendPercentage { get; set; } 
    }
}
