using IntelliTect.Coalesce;
using System.Linq;

namespace Coalesce.Domain
{
    [Coalesce, StandaloneEntity]
    public class CaseStandalone
    {
        public int Id { get; private set; }

        public Person? AssignedTo { get; private set; }

        [DefaultDataSource]
        public class DefaultSource(CrudContext<AppDbContext> context) : StandardDataSource<CaseStandalone, AppDbContext>(context)
        {
            public override IQueryable<CaseStandalone> GetQuery(IDataSourceParameters parameters)
            {
                return Db.Cases
                    .Select(c => new CaseStandalone
                    {
                        Id = c.CaseKey,
                        AssignedTo = c.AssignedTo
                    });
            }
        }
    }
}
