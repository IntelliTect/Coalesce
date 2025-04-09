using System.Collections.Generic;

namespace IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext
{
    public class MultipleParents
    {
        public int Id { get; set; }

        public int? Parent1Id { get; set; }
        public Parent1 Parent1 { get; set; }

        public int? Parent2Id { get; set; }
        public Parent2 Parent2 { get; set; }
    }

    public class Parent1
    {
        public int Id { get; set; }
        public List<MultipleParents> Children { get; set; }
    }

    public class Parent2
    {
        public int Id { get; set; }
        public List<MultipleParents> Children { get; set; }
    }
}
