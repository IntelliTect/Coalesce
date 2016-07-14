using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Data
{
    public class Fingerprintable<TUser, TKey>: IFingerprintable<TUser, TKey>
    {
        public TUser CreatedBy { get; set; }
        public TKey CreatedById { get; set; }
        public DateTimeOffset CreatedOn { get; set; }

        public TUser ModifiedBy { get; set; }
        public TKey ModifiedById { get; set; }
        public DateTimeOffset ModifiedOn { get; set; }
    }
}
