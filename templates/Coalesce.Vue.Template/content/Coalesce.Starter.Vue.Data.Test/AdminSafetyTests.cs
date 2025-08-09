using Coalesce.Starter.Vue.Data.Models;
using IntelliTect.Coalesce;
using Microsoft.AspNetCore.Identity;

namespace Coalesce.Starter.Vue.Data.Test;

public class AdminSafetyTests : TestBase
{
    [Fact]
    public async Task Role_CannotRemoveUserAdminPermission_WhenItWouldLeaveNoAdmins()
    {
        // Arrange
        var adminRole = new Role
        {
            Id = "admin-role",
            Name = "Admin",
            NormalizedName = "ADMIN",
            Permissions = new List<Permission> { Permission.UserAdmin, Permission.Admin }
        };
        
        var user = new User 
        { 
            Id = "user1", 
            UserName = "admin@test.com",
            NormalizedUserName = "ADMIN@TEST.COM",
            Email = "admin@test.com"
        };
        
        var userRole = new UserRole { UserId = user.Id, RoleId = adminRole.Id };
        
        Db.Roles.Add(adminRole);
        Db.Users.Add(user);
        Db.UserRoles.Add(userRole);
        await Db.SaveChangesAsync();

        RefreshServices();

        // Get fresh instances after saving
        var roleToUpdate = await Db.Roles.FindAsync(adminRole.Id);
        var behaviors = new Role.Behaviors(Mocker.Get<RoleManager<Role>>(), Mocker.Get<CrudContext<AppDbContext>>());

        // Act & Assert
        roleToUpdate!.Permissions = new List<Permission> { Permission.Admin }; // Remove UserAdmin permission
        var result = behaviors.BeforeSave(SaveKind.Update, adminRole, roleToUpdate);
        
        Assert.False(result.WasSuccessful);
        Assert.Contains("no user administrators", result.Message);
    }

    [Fact]
    public async Task Role_CanRemoveUserAdminPermission_WhenOtherAdminsExist()
    {
        // Arrange
        var adminRole1 = new Role
        {
            Id = "admin-role-1",
            Name = "Admin1",
            NormalizedName = "ADMIN1",
            Permissions = new List<Permission> { Permission.UserAdmin }
        };
        
        var adminRole2 = new Role
        {
            Id = "admin-role-2", 
            Name = "Admin2",
            NormalizedName = "ADMIN2",
            Permissions = new List<Permission> { Permission.UserAdmin }
        };

        var user1 = new User 
        { 
            Id = "user1", 
            UserName = "admin1@test.com",
            NormalizedUserName = "ADMIN1@TEST.COM"
        };
        
        var user2 = new User 
        { 
            Id = "user2", 
            UserName = "admin2@test.com", 
            NormalizedUserName = "ADMIN2@TEST.COM"
        };

        var userRole1 = new UserRole { UserId = user1.Id, RoleId = adminRole1.Id };
        var userRole2 = new UserRole { UserId = user2.Id, RoleId = adminRole2.Id };

        Db.Roles.AddRange(adminRole1, adminRole2);
        Db.Users.AddRange(user1, user2);
        Db.UserRoles.AddRange(userRole1, userRole2);
        await Db.SaveChangesAsync();

        RefreshServices();

        var roleToUpdate = await Db.Roles.FindAsync(adminRole1.Id);
        var behaviors = new Role.Behaviors(Mocker.Get<RoleManager<Role>>(), Mocker.Get<CrudContext<AppDbContext>>());

        // Act
        roleToUpdate!.Permissions = new List<Permission> { Permission.Admin }; // Remove UserAdmin permission
        var result = behaviors.BeforeSave(SaveKind.Update, adminRole1, roleToUpdate);

        // Assert
        Assert.True(result.WasSuccessful);
    }

    [Fact]
    public async Task Role_CannotDeleteRole_WhenItWouldLeaveNoAdmins()
    {
        // Arrange
        var adminRole = new Role
        {
            Id = "admin-role",
            Name = "Admin",
            NormalizedName = "ADMIN",
            Permissions = new List<Permission> { Permission.UserAdmin }
        };
        
        var user = new User 
        { 
            Id = "user1", 
            UserName = "admin@test.com",
            NormalizedUserName = "ADMIN@TEST.COM"
        };
        
        var userRole = new UserRole { UserId = user.Id, RoleId = adminRole.Id };
        
        Db.Roles.Add(adminRole);
        Db.Users.Add(user);
        Db.UserRoles.Add(userRole);
        await Db.SaveChangesAsync();

        RefreshServices();

        var roleToDelete = await Db.Roles.FindAsync(adminRole.Id);
        var behaviors = new Role.Behaviors(Mocker.Get<RoleManager<Role>>(), Mocker.Get<CrudContext<AppDbContext>>());

        // Act & Assert
        var result = behaviors.BeforeDelete(roleToDelete!);
        
        Assert.False(result.WasSuccessful);
        Assert.Contains("no user administrators", result.Message);
    }

    [Fact]
    public async Task Role_CanDeleteRole_WhenOtherAdminsExist()
    {
        // Arrange
        var adminRole1 = new Role
        {
            Id = "admin-role-1",
            Name = "Admin1",
            NormalizedName = "ADMIN1",
            Permissions = new List<Permission> { Permission.UserAdmin }
        };
        
        var adminRole2 = new Role
        {
            Id = "admin-role-2",
            Name = "Admin2", 
            NormalizedName = "ADMIN2",
            Permissions = new List<Permission> { Permission.UserAdmin }
        };

        var user1 = new User 
        { 
            Id = "user1", 
            UserName = "admin1@test.com",
            NormalizedUserName = "ADMIN1@TEST.COM"
        };
        
        var user2 = new User 
        { 
            Id = "user2", 
            UserName = "admin2@test.com",
            NormalizedUserName = "ADMIN2@TEST.COM"
        };

        var userRole1 = new UserRole { UserId = user1.Id, RoleId = adminRole1.Id };
        var userRole2 = new UserRole { UserId = user2.Id, RoleId = adminRole2.Id };

        Db.Roles.AddRange(adminRole1, adminRole2);
        Db.Users.AddRange(user1, user2);
        Db.UserRoles.AddRange(userRole1, userRole2);
        await Db.SaveChangesAsync();

        RefreshServices();

        var roleToDelete = await Db.Roles.FindAsync(adminRole1.Id);
        var behaviors = new Role.Behaviors(Mocker.Get<RoleManager<Role>>(), Mocker.Get<CrudContext<AppDbContext>>());

        // Act
        var result = behaviors.BeforeDelete(roleToDelete!);

        // Assert
        Assert.True(result.WasSuccessful);
    }

    [Fact]
    public async Task UserRole_CannotDeleteUserRole_WhenItWouldLeaveNoAdmins()
    {
        // Arrange
        var adminRole = new Role
        {
            Id = "admin-role",
            Name = "Admin",
            NormalizedName = "ADMIN",
            Permissions = new List<Permission> { Permission.UserAdmin }
        };
        
        var user = new User 
        { 
            Id = "user1", 
            UserName = "admin@test.com",
            NormalizedUserName = "ADMIN@TEST.COM"
        };
        
        var userRole = new UserRole { UserId = user.Id, RoleId = adminRole.Id, Role = adminRole, User = user };
        
        Db.Roles.Add(adminRole);
        Db.Users.Add(user);
        Db.UserRoles.Add(userRole);
        await Db.SaveChangesAsync();

        RefreshServices();

        var userRoleToDelete = await Db.UserRoles
            .Include(ur => ur.Role)
            .FirstAsync(ur => ur.UserId == user.Id && ur.RoleId == adminRole.Id);
        var behaviors = new UserRole.Behaviors(Mocker.Get<CrudContext<AppDbContext>>(), Mocker.Get<SignInManager<User>>());

        // Act & Assert
        var result = behaviors.BeforeDelete(userRoleToDelete);
        
        Assert.False(result.WasSuccessful);
        Assert.Contains("no user administrators", result.Message);
    }

    [Fact]
    public async Task UserRole_CanDeleteUserRole_WhenOtherAdminsExist()
    {
        // Arrange
        var adminRole = new Role
        {
            Id = "admin-role",
            Name = "Admin",
            NormalizedName = "ADMIN",
            Permissions = new List<Permission> { Permission.UserAdmin }
        };

        var user1 = new User 
        { 
            Id = "user1", 
            UserName = "admin1@test.com",
            NormalizedUserName = "ADMIN1@TEST.COM"
        };
        
        var user2 = new User 
        { 
            Id = "user2", 
            UserName = "admin2@test.com",
            NormalizedUserName = "ADMIN2@TEST.COM"
        };

        var userRole1 = new UserRole { UserId = user1.Id, RoleId = adminRole.Id, Role = adminRole, User = user1 };
        var userRole2 = new UserRole { UserId = user2.Id, RoleId = adminRole.Id, Role = adminRole, User = user2 };

        Db.Roles.Add(adminRole);
        Db.Users.AddRange(user1, user2);
        Db.UserRoles.AddRange(userRole1, userRole2);
        await Db.SaveChangesAsync();

        RefreshServices();

        var userRoleToDelete = await Db.UserRoles
            .Include(ur => ur.Role)
            .FirstAsync(ur => ur.UserId == user1.Id && ur.RoleId == adminRole.Id);
        var behaviors = new UserRole.Behaviors(Mocker.Get<CrudContext<AppDbContext>>(), Mocker.Get<SignInManager<User>>());

        // Act
        var result = behaviors.BeforeDelete(userRoleToDelete);

        // Assert
        Assert.True(result.WasSuccessful);
    }

#if Tenancy
    [Fact]
    public async Task Role_CanRemoveUserAdminPermission_WhenGlobalAdminExists()
    {
        // Arrange
        var adminRole = new Role
        {
            Id = "admin-role",
            Name = "Admin",
            NormalizedName = "ADMIN", 
            Permissions = new List<Permission> { Permission.UserAdmin }
        };
        
        var regularUser = new User 
        { 
            Id = "user1", 
            UserName = "admin@test.com",
            NormalizedUserName = "ADMIN@TEST.COM"
        };
        
        var globalAdmin = new User 
        { 
            Id = "user2", 
            UserName = "global@test.com",
            NormalizedUserName = "GLOBAL@TEST.COM",
            IsGlobalAdmin = true
        };
        
        var userRole = new UserRole { UserId = regularUser.Id, RoleId = adminRole.Id };
        
        Db.Roles.Add(adminRole);
        Db.Users.AddRange(regularUser, globalAdmin);
        Db.UserRoles.Add(userRole);
        await Db.SaveChangesAsync();

        RefreshServices();

        var roleToUpdate = await Db.Roles.FindAsync(adminRole.Id);
        var behaviors = new Role.Behaviors(Mocker.Get<RoleManager<Role>>(), Mocker.Get<CrudContext<AppDbContext>>());

        // Act
        roleToUpdate!.Permissions = new List<Permission> { Permission.Admin }; // Remove UserAdmin permission
        var result = behaviors.BeforeSave(SaveKind.Update, adminRole, roleToUpdate);

        // Assert
        Assert.True(result.WasSuccessful);
    }

    [Fact]
    public async Task UserRole_CanDeleteUserRole_WhenGlobalAdminExists()
    {
        // Arrange
        var adminRole = new Role
        {
            Id = "admin-role",
            Name = "Admin",
            NormalizedName = "ADMIN",
            Permissions = new List<Permission> { Permission.UserAdmin }
        };
        
        var regularUser = new User 
        { 
            Id = "user1", 
            UserName = "admin@test.com",
            NormalizedUserName = "ADMIN@TEST.COM"
        };
        
        var globalAdmin = new User 
        { 
            Id = "user2", 
            UserName = "global@test.com",
            NormalizedUserName = "GLOBAL@TEST.COM",
            IsGlobalAdmin = true
        };
        
        var userRole = new UserRole { UserId = regularUser.Id, RoleId = adminRole.Id, Role = adminRole, User = regularUser };
        
        Db.Roles.Add(adminRole);
        Db.Users.AddRange(regularUser, globalAdmin);
        Db.UserRoles.Add(userRole);
        await Db.SaveChangesAsync();

        RefreshServices();

        var userRoleToDelete = await Db.UserRoles
            .Include(ur => ur.Role)
            .FirstAsync(ur => ur.UserId == regularUser.Id && ur.RoleId == adminRole.Id);
        var behaviors = new UserRole.Behaviors(Mocker.Get<CrudContext<AppDbContext>>(), Mocker.Get<SignInManager<User>>());

        // Act
        var result = behaviors.BeforeDelete(userRoleToDelete);

        // Assert
        Assert.True(result.WasSuccessful);
    }
#endif
}