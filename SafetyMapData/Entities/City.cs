using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafetyMapData.Entities
{
    public class City
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int Population { get; set; }

        public ICollection<Neighborhood> Neighborhoods { get; set; } = new List<Neighborhood>();
    }
}
