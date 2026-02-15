namespace IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;

public class Sibling
{
    public int SiblingId { get; set; }

    public int PersonId { get; set; }
    public Person Person { get; set; }

    public int PersonTwoId { get; set; }
    public Person PersonTwo { get; set; }
}
