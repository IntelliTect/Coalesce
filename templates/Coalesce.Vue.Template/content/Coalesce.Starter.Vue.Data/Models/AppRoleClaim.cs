using Coalesce.Starter.Vue.Data.Models;
using Microsoft.AspNetCore.Identity;

[Edit(DenyAll)]
[Create(DenyAll)]
[Delete(DenyAll)]
public class AppRoleClaim : IdentityRoleClaim<string>
{
    [ForeignKey(nameof(RoleId))]
    public AppRole? Role { get; set; }
}