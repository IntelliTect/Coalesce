using Coalesce.Starter.Vue.Data.Coalesce;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Security;

namespace Coalesce.Starter.Vue.Data.Models;

[Create(DenyAll)]
[Edit(DenyAll)]
[Delete(DenyAll)]
public class AppRole : IdentityRole
{
    [Required, Search(SearchMethod = SearchAttribute.SearchMethods.Contains)]
    public override string? Name { get; set; }

    [InternalUse]
    public override string? ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();

    [InternalUse]
    public override string? NormalizedName { get; set; }

    [Hidden(HiddenAttribute.Areas.List)]
    [InverseProperty(nameof(AppRoleClaim.Role))]
    public ICollection<AppRoleClaim>? RoleClaims { get; set; }

    [NotMapped, Hidden]
    public Permission[] Permissions => 
        RoleClaims is null 
            ? Array.Empty<Permission>() 
        : RoleClaims
            .Where(c => c.ClaimType == AppClaimTypes.Permission && Enum.TryParse<Permission>(c.ClaimValue, out _))
            .Select(r => Enum.Parse<Permission>(r.ClaimValue))
            .ToArray();


    public class Behaviors(RoleManager<AppRole> roleManager, CrudContext<AppDbContext> context) : AppBehaviors<AppRole>(context)
    {
        public override ItemResult BeforeSave(SaveKind kind, AppRole? oldItem, AppRole item)
        {
            item.NormalizedName = roleManager.NormalizeKey(item.Name);

            return base.BeforeSave(kind, oldItem, item);
        }
    }
}