using Coalesce.Starter.Vue.Data.Coalesce;
using Microsoft.AspNetCore.Identity;

namespace Coalesce.Starter.Vue.Data.Models;

[Create(nameof(Permission.UserAdmin))]
[Edit(nameof(Permission.UserAdmin))]
[Delete(nameof(Permission.UserAdmin))]
public class Role : IdentityRole
{
    [Required, Search(SearchMethod = SearchMethods.Contains)]
    public override string? Name { get; set; }

    [InternalUse]
    public override string? ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();

    [InternalUse]
    public override string? NormalizedName { get; set; }

    [Hidden(HiddenAttribute.Areas.List)]
    [InverseProperty(nameof(RoleClaim.Role))]
    public ICollection<RoleClaim>? RoleClaims { get; set; }

    // TODO: Permission editing
    [NotMapped, Hidden]
    public Permission[] Permissions => 
        RoleClaims is null 
            ? Array.Empty<Permission>() 
        : RoleClaims
            .Where(c => c.ClaimType == AppClaimTypes.Permission && Enum.TryParse<Permission>(c.ClaimValue, out _))
            .Select(r => Enum.Parse<Permission>(r.ClaimValue))
            .ToArray();


    public class Behaviors(RoleManager<Role> roleManager, CrudContext<AppDbContext> context) : AppBehaviors<Role>(context)
    {
        public override ItemResult BeforeSave(SaveKind kind, Role? oldItem, Role item)
        {
            item.NormalizedName = roleManager.NormalizeKey(item.Name);

            return base.BeforeSave(kind, oldItem, item);
        }
    }
}