using Microsoft.AspNetCore.Identity;

namespace Coalesce.Starter.Vue.Data.Models;

[Read("UserAdmin")]
[Edit(DenyAll)] // Doesn't make sense to edit. 
[Create("UserAdmin")]
[Delete("UserAdmin")]
public class AppUserRole : IdentityUserRole<string>
{
    // Fake PK for Coalesce since IdentityUserRole uses a composite PK.
    [NotMapped]
    public string Id
    {
        get => $"{UserId};{RoleId}";
        set
        {
            var split = value?.Split(';') ?? new string[2];
            UserId = split[0];
            RoleId = split[1];
        }
    }

    [DefaultOrderBy(FieldOrder = 0)]
    public virtual AppUser User { get; set; }

    [DefaultOrderBy(FieldOrder = 1)]
    public virtual AppRole Role { get; set; }

    [DefaultDataSource]
    public class DefaultSource(CrudContext<AppDbContext> context) : StandardDataSource<AppUserRole, AppDbContext>(context)
    {
        // We have to teach Coalesce how to properly query for our compound key.
        protected override Task<AppUserRole> EvaluateItemQueryAsync(
            object id,
            IQueryable<AppUserRole> query,
            CancellationToken cancellationToken = default)
        {
            var idParts = id.ToString().Split(";");
            return query.FirstOrDefaultAsync(r => r.UserId == idParts[0] && r.RoleId == idParts[1], cancellationToken);
        }
    }
}