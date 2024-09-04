using Microsoft.AspNetCore.Identity;

namespace Coalesce.Starter.Vue.Data.Models;

[Read(nameof(Permission.UserAdmin))]
[Edit(DenyAll)] // Doesn't make sense to edit. 
[Create(nameof(Permission.UserAdmin))]
[Delete(nameof(Permission.UserAdmin))]
public class UserRole : IdentityUserRole<string>
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
    public User? User { get; set; }

    [DefaultOrderBy(FieldOrder = 1)]
    public Role? Role { get; set; }

    [DefaultDataSource]
    public class DefaultSource(CrudContext<AppDbContext> context) : StandardDataSource<UserRole, AppDbContext>(context)
    {
        // Teach Coalesce how to properly query for our compound key:
        protected override Task<UserRole?> EvaluateItemQueryAsync(
            object id,
            IQueryable<UserRole> query,
            CancellationToken cancellationToken = default)
        {
            var idParts = id.ToString()!.Split(";");
            return query.FirstOrDefaultAsync(r => r.UserId == idParts[0] && r.RoleId == idParts[1], cancellationToken);
        }
    }
}