namespace Coalesce.Starter.Vue.Data.Models;

[InternalUse]
[Index(nameof(UserId), IsUnique = true)]
public class UserPhoto
#if TrackingBase
    : TrackingBase
#endif
{
    public int UserPhotoId { get; set; }

    public required string UserId { get; set; }
    public User? User { get; set; }

    public required byte[] Content { get; set; }
}