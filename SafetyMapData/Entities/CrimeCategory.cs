using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafetyMapData.Entities
{
    public class CrimeCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; 
        public string ColorCode { get; set; } = string.Empty;

        public ICollection<CrimeStatistic> Statistics { get; set; } = new List<CrimeStatistic>();
    }
}
