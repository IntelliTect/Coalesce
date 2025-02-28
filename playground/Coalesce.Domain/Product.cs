using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IntelliTect.Coalesce.DataAnnotations;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace Coalesce.Domain
{
    [Table("Product")]
    [Create(Roles = "Admin")]
    [Edit(Roles = "Admin")]
    [Description("A product that can be purchased.")]
    [Index(nameof(UniqueId), IsUnique = true)]
    public class Product
    {
        public int ProductId { get; set; }

        [Search(SearchMethod = SearchAttribute.SearchMethods.Contains)]
        public string Name { get; set; }

        [Required]
        public ProductDetails Details { get; set; }

        [Column("ProductUniqueId")]
        [Read]
        [Edit(Roles = "Admin")]
        [DataType(DataType.Password)]
        public Guid UniqueId { get; set; } = Guid.NewGuid();

        [NotMapped]
        public object Unknown { get; set; } = "unknown value";
    }

    public class ProductDetails
    {
        [ListText]
        public StreetAddress ManufacturingAddress { get; set; }

        public StreetAddress CompanyHqAddress { get; set; }
    }

    public class StreetAddress
    {
        [ListText]
        public string Address { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string PostalCode { get; set; }

    }
}