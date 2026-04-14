using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IntelliTect.Coalesce.DataAnnotations;

namespace IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;

public enum Grade
{
    Freshman = 9,
    Sophomore = 10,
    Junior = 11,
    Senior = 12
}

[Description("A student entity used to test edge cases with FK/nav relationships.")]
public class Student
{
    public int StudentId { get; set; }

    public string Name { get; set; }

    public bool IsEnrolled { get; set; }

    public System.DateTime? BirthDate { get; set; }

    [InverseProperty(nameof(Course.Student))]
    public ICollection<Course> Courses { get; set; }

    // Declare nav BEFORE FK to test that the framework handles nav-before-FK ordering.
    public Course CurrentCourse { get; set; }
    public int? CurrentCourseId { get; set; }

    public Grade? Grade { get; set; }

    // Declare nav BEFORE FK (this is the critical edge case).
    // FK is intentionally named weirdly as to not match the name of the PK of Advisor.
    // This helps eek out bugs where the wrong props names are used for setting/getting props.
    [ForeignKey(nameof(StudentAdvisorId))]
    public Advisor Advisor { get; set; }

    [ForeignKey(nameof(Advisor))]
    public int? StudentAdvisorId { get; set; }
}

public class Advisor
{
    public int AdvisorId { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    public string Name { get; set; }

    [InverseProperty(nameof(Student.Advisor))]
    public ICollection<Student> Students { get; set; }

    /// <summary>
    /// A collection of models that doesn't function as a navigation property.
    /// </summary>
    [NotMapped]
    public IEnumerable<Student> StudentsNonNavigation => Students;

    /// <summary>
    /// An object type property containing a model reference.
    /// </summary>
    [NotMapped]
    public StudentWrapper StudentWrapperObject { get; set; }
}

public class Course
{
    public int CourseId { get; set; }

    public string Name { get; set; }

    public int? StudentId { get; set; }
    public Student Student { get; set; }
}

/// <summary>
/// Object type (not a DB entity) that wraps a Student model reference.
/// Used to verify object types can contain model references.
/// </summary>
public class StudentWrapper
{
    public string Name { get; set; }

    public Student Student { get; set; }
}
