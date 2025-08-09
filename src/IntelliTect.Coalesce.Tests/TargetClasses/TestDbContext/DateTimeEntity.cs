using IntelliTect.Coalesce.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations;

namespace IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext
{
    /// <summary>
    /// Test entity with DateTime primary key to test date-based primary key support
    /// </summary>
    public class DateTimeEntity
    {
        [Key]
        public DateTime DateTimeId { get; set; }
        
        public string? Name { get; set; }
        
        public string? Description { get; set; }
    }

    /// <summary>
    /// Test entity with DateOnly primary key to test date-based primary key support
    /// </summary>
    public class DateOnlyEntity
    {
        [Key]
        public DateOnly DateOnlyId { get; set; }
        
        public string? Name { get; set; }
        
        public string? Description { get; set; }
    }

    /// <summary>
    /// Test entity with DateTimeOffset primary key to test date-based primary key support
    /// </summary>
    public class DateTimeOffsetEntity
    {
        [Key]
        public DateTimeOffset DateTimeOffsetId { get; set; }
        
        public string? Name { get; set; }
        
        public string? Description { get; set; }
    }
}