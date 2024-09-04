using Coalesce.Starter.Vue.Data.Models;
using Microsoft.AspNetCore.Identity;

[Edit(DenyAll)]
[Create(DenyAll)]
[Delete(DenyAll)]
public class RoleClaim : IdentityRoleClaim<string>
{
    [ForeignKey(nameof(RoleId))]
    public Role? Role { get; set; }
}