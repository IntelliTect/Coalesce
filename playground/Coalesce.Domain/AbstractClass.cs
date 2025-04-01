namespace Coalesce.Domain
{
    public abstract class AbstractClass
    {
        public int Id { get; set; }

        public string? AbstractClassString { get; set; }
    }

    public class AbstractClassImpl : AbstractClass
    {
        public string? ImplString { get; set; }
    }
}
