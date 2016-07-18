using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Data
{
    public interface IFingerprintable<TUser, TKey>
    {
        TUser CreatedBy { get; set; }
        TKey CreatedById { get; set; }
        DateTimeOffset CreatedOn { get; set; }

        TUser ModifiedBy { get; set; }
        TKey ModifiedById { get; set; }
        DateTimeOffset ModifiedOn { get; set; }
    }
}
