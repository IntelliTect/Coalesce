using IntelliTect.Coalesce.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Coalesce.Domain
{
    public class ZipCode
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [ListText]
        public string Zip { get; set; }

        public string State { get; set; }
    }
}
