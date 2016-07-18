namespace IntelliTect.Coalesce.Models
{
    public class ValidationIssue
    {
        public string Property { get; set; }
        public string Issue { get; set; }

        public ValidationIssue(string property, string issue)
        {
            Property = property;
            Issue = issue;
        }
    }
}