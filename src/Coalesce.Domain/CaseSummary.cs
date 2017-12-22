using IntelliTect.Coalesce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Coalesce.Domain
{
    public class CaseSummary
    {
        public int CaseSummaryId { get; set; }
        public int OpenCases { get; set; }
        public int CaseCount { get; set; }
        public int CloseCases { get; set; }
        public string Description { get; set; }

        [Coalesce]
        public static CaseSummary GetCaseSummary(AppDbContext db)
        {
            return new CaseSummary()
            {
                CaseSummaryId = 1,
                OpenCases = 2,
                CaseCount = db.Cases.Count(),
                CloseCases = 4,
                Description = "This is a test!"
            };
        }
    }
}
