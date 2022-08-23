using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Coalesce.Domain
{
    [Table("PersonLocation")]
    public class PersonLocation
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
