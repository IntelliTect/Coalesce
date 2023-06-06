namespace IntelliTect.Coalesce.Tests.TargetClasses
{
    public abstract class AbstractModel
    {
        public int Id { get; set; }

        public string Discriminatior { get; set; }
    }

    public class AbstractImpl : AbstractModel
    {
        public string ImplOnlyField { get; set; }
    }
}
