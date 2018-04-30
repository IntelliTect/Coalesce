using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Coalesce.Domain
{
    [NotMapped]
    public class PersonStats
    {
        //public int PersonStatsId { get; set; }
        public double Height { get; set; }
        public double Weight { get; set; }
        public string Name { get; set; }

        // public PersonLocation PersonLocation { get; set; }
    }


}
