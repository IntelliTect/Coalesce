using IntelliTect.Coalesce.Utilities;

namespace Coalesce.Starter.Vue.Data.Models;

public abstract class TrackingBase
{
    [ForeignKey("ModifiedById")]
    [Read, Display(Order = 1000010)]
    public AppUser? ModifiedBy { get; set; }
    [Read]
    public string? ModifiedById { get; set; }

    [Read, Display(Order = 1000012)]
    public DateTimeOffset ModifiedOn { get; set; }

    [ForeignKey("CreatedById")]
    [Read, Display(Order = 1000000)]
    public virtual AppUser? CreatedBy { get; set; }
    [Read]
    public string? CreatedById { get; set; }

    [Read, Display(Order = 1000002)]
    public virtual DateTimeOffset CreatedOn { get; set; }

    [InternalUse]
    public void SetTracking(string? userIdentityId)
    {
        if (CreatedById == null)
        {
            CreatedById = userIdentityId;
        }
        if (CreatedOn == default)
        {
            // CreatedOn is handled separately so that we can avoid resetting the 
            // CreatedOn stamp if the entity wasn't created by a user.
            CreatedOn = DateTimeOffset.Now;
        }

        ModifiedById = userIdentityId;
        ModifiedOn = DateTimeOffset.Now;
    }

    public void SetTracking(ClaimsPrincipal? user) => SetTracking(user?.GetUserId());
}