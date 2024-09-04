using IntelliTect.Coalesce.Utilities;

namespace Coalesce.Starter.Vue.Data.Models;

public abstract class TrackingBase
{
#if Identity
    [ForeignKey("ModifiedById")]
    [Read, Display(Order = 1000010)]
    public User? ModifiedBy { get; set; }
#endif

    [Read]
    public string? ModifiedById { get; set; }

    [Read, Display(Order = 1000012)]
    public DateTimeOffset ModifiedOn { get; set; }

#if Identity
    [ForeignKey("CreatedById")]
    [Read, Display(Order = 1000000)]
    public User? CreatedBy { get; set; }
#endif

    [Read]
    public string? CreatedById { get; set; }

    [Read, Display(Order = 1000002)]
    public DateTimeOffset CreatedOn { get; set; }

    [InternalUse]
    public void SetTracking(string? userId)
    {
        if (CreatedById == null)
        {
            CreatedById = userId;
        }
        if (CreatedOn == default)
        {
            // CreatedOn is handled separately so that we can avoid resetting the 
            // CreatedOn stamp if the entity wasn't created by a user.
            CreatedOn = DateTimeOffset.Now;
        }

        ModifiedById = userId;
        ModifiedOn = DateTimeOffset.Now;
    }

    public void SetTracking(ClaimsPrincipal? user) => SetTracking(user?.GetUserId());
}