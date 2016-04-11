namespace Intellitect.ComponentModel.DataAnnotations
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class DetailAttribute : System.Attribute
    {
        public string Detail { get; set; }

        public DetailAttribute(string detail)
        {
            this.Detail = detail;
        }
    }
}