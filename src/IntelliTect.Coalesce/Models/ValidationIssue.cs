using System;

namespace IntelliTect.Coalesce.Models;

public class ValidationIssue
{
    public string Property { get; set; }
    public string Issue { get; set; }

    public ValidationIssue(string property, string issue)
    {
        Property = property ?? throw new ArgumentNullException(nameof(property));
        Issue = issue ?? throw new ArgumentNullException(nameof(issue));
    }
}
