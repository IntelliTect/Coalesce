using System.Security.Claims;
using IntelliTect.Coalesce;
using Microsoft.AspNetCore.Identity;

namespace Coalesce.Starter.Vue.Data.Test;

public class AdminSafetyTests : TestBase
{
    [Fact]
    public void Role_BeforeDelete_PreventsDeletingLastAdminRole()
    {
        // Arrange - Setup a single admin role with a user
        var adminRole = new Role
        {
            Id = "admin-role-id",
            Name = "Admin",
            NormalizedName = "ADMIN",
            Permissions = [Permission.UserAdmin]
        };

        var user = new User
        {
            Id = "user-id",
            UserName = "admin@test.com",
            NormalizedUserName = "ADMIN@TEST.COM"
        };

        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = adminRole.Id,
            User = user,
            Role = adminRole
        };

        Db.Roles.Add(adminRole);
        Db.Users.Add(user);
        Db.UserRoles.Add(userRole);
        Db.SaveChanges();

        RefreshServices();

        // Get a properly configured behaviors instance
        var roleManager = Mocker.Get<RoleManager<Role>>();
        var crudContext = Mocker.Get<CrudContext<AppDbContext>>();
        var roleBehaviors = new Role.Behaviors(roleManager, crudContext);

        // Act & Assert
        var result = roleBehaviors.BeforeDelete(adminRole);
        
        Assert.False(result.WasSuccessful);
        Assert.Contains("cannot delete", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("remaining user admins", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Role_BeforeDelete_AllowsDeletingNonAdminRole()
    {
        // Arrange
        var adminRole = new Role
        {
            Id = "admin-role-id",
            Name = "Admin",
            NormalizedName = "ADMIN",
            Permissions = [Permission.UserAdmin]
        };

        var regularRole = new Role
        {
            Id = "regular-role-id",
            Name = "Regular",
            NormalizedName = "REGULAR",
            Permissions = [Permission.ViewAuditLogs]
        };

        var user = new User
        {
            Id = "user-id",
            UserName = "admin@test.com",
            NormalizedUserName = "ADMIN@TEST.COM"
        };

        var adminUserRole = new UserRole
        {
            UserId = user.Id,
            RoleId = adminRole.Id,
            User = user,
            Role = adminRole
        };

        Db.Roles.AddRange(adminRole, regularRole);
        Db.Users.Add(user);
        Db.UserRoles.Add(adminUserRole);
        Db.SaveChanges();

        RefreshServices();

        var roleManager = Mocker.Get<RoleManager<Role>>();
        var crudContext = Mocker.Get<CrudContext<AppDbContext>>();
        var roleBehaviors = new Role.Behaviors(roleManager, crudContext);

        // Act
        var result = roleBehaviors.BeforeDelete(regularRole);
        
        // Assert
        Assert.True(result.WasSuccessful);
    }

    [Fact]
    public void Role_BeforeDelete_AllowsDeletingAdminRoleWhenOtherAdminsExist()
    {
        // Arrange
        var adminRole1 = new Role
        {
            Id = "admin-role-1",
            Name = "Admin1",
            NormalizedName = "ADMIN1",
            Permissions = [Permission.UserAdmin]
        };

        var adminRole2 = new Role
        {
            Id = "admin-role-2", 
            Name = "Admin2",
            NormalizedName = "ADMIN2",
            Permissions = [Permission.UserAdmin]
        };

        var user1 = new User
        {
            Id = "user-1",
            UserName = "admin1@test.com",
            NormalizedUserName = "ADMIN1@TEST.COM"
        };

        var user2 = new User
        {
            Id = "user-2",
            UserName = "admin2@test.com", 
            NormalizedUserName = "ADMIN2@TEST.COM"
        };

        var userRole1 = new UserRole
        {
            UserId = user1.Id,
            RoleId = adminRole1.Id,
            User = user1,
            Role = adminRole1
        };

        var userRole2 = new UserRole
        {
            UserId = user2.Id,
            RoleId = adminRole2.Id,
            User = user2,
            Role = adminRole2
        };

        Db.Roles.AddRange(adminRole1, adminRole2);
        Db.Users.AddRange(user1, user2);
        Db.UserRoles.AddRange(userRole1, userRole2);
        Db.SaveChanges();

        RefreshServices();

        var roleManager = Mocker.Get<RoleManager<Role>>();
        var crudContext = Mocker.Get<CrudContext<AppDbContext>>();
        var roleBehaviors = new Role.Behaviors(roleManager, crudContext);

        // Act
        var result = roleBehaviors.BeforeDelete(adminRole1);
        
        // Assert
        Assert.True(result.WasSuccessful);
    }

    [Fact]
    public void Role_BeforeSave_PreventsRemovingUserAdminFromLastAdminRole()
    {
        // Arrange
        var adminRole = new Role
        {
            Id = "admin-role-id",
            Name = "Admin",
            NormalizedName = "ADMIN", 
            Permissions = [Permission.UserAdmin]
        };

        var user = new User
        {
            Id = "user-id",
            UserName = "admin@test.com",
            NormalizedUserName = "ADMIN@TEST.COM"
        };

        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = adminRole.Id,
            User = user,
            Role = adminRole
        };

        Db.Roles.Add(adminRole);
        Db.Users.Add(user);
        Db.UserRoles.Add(userRole);
        Db.SaveChanges();

        RefreshServices();

        var roleManager = Mocker.Get<RoleManager<Role>>();
        var crudContext = Mocker.Get<CrudContext<AppDbContext>>();
        var roleBehaviors = new Role.Behaviors(roleManager, crudContext);

        // Create updated role without UserAdmin permission
        var updatedRole = new Role
        {
            Id = adminRole.Id,
            Name = adminRole.Name,
            NormalizedName = adminRole.NormalizedName,
            Permissions = [Permission.ViewAuditLogs] // Removed UserAdmin
        };

        // Act
        var result = roleBehaviors.BeforeSave(SaveKind.Update, adminRole, updatedRole);
        
        // Assert
        Assert.False(result.WasSuccessful);
        Assert.Contains("cannot remove", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("UserAdmin permission", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Role_BeforeSave_AllowsRemovingUserAdminWhenOtherAdminsExist()
    {
        // Arrange
        var adminRole1 = new Role
        {
            Id = "admin-role-1",
            Name = "Admin1",
            NormalizedName = "ADMIN1",
            Permissions = [Permission.UserAdmin]
        };

        var adminRole2 = new Role
        {
            Id = "admin-role-2",
            Name = "Admin2", 
            NormalizedName = "ADMIN2",
            Permissions = [Permission.UserAdmin]
        };

        var user1 = new User
        {
            Id = "user-1",
            UserName = "admin1@test.com",
            NormalizedUserName = "ADMIN1@TEST.COM"
        };

        var user2 = new User
        {
            Id = "user-2",
            UserName = "admin2@test.com",
            NormalizedUserName = "ADMIN2@TEST.COM"
        };

        var userRole1 = new UserRole
        {
            UserId = user1.Id,
            RoleId = adminRole1.Id,
            User = user1,
            Role = adminRole1
        };

        var userRole2 = new UserRole
        {
            UserId = user2.Id,
            RoleId = adminRole2.Id,
            User = user2,
            Role = adminRole2
        };

        Db.Roles.AddRange(adminRole1, adminRole2);
        Db.Users.AddRange(user1, user2);
        Db.UserRoles.AddRange(userRole1, userRole2);
        Db.SaveChanges();

        RefreshServices();

        var roleManager = Mocker.Get<RoleManager<Role>>();
        var crudContext = Mocker.Get<CrudContext<AppDbContext>>();
        var roleBehaviors = new Role.Behaviors(roleManager, crudContext);

        // Create updated role without UserAdmin permission
        var updatedRole = new Role
        {
            Id = adminRole1.Id,
            Name = adminRole1.Name,
            NormalizedName = adminRole1.NormalizedName,
            Permissions = [Permission.ViewAuditLogs] // Removed UserAdmin
        };

        // Act
        var result = roleBehaviors.BeforeSave(SaveKind.Update, adminRole1, updatedRole);
        
        // Assert
        Assert.True(result.WasSuccessful);
    }

    [Fact]
    public void UserRole_BeforeDelete_PreventsRemovingLastAdmin()
    {
        // Arrange
        var adminRole = new Role
        {
            Id = "admin-role-id",
            Name = "Admin",
            NormalizedName = "ADMIN",
            Permissions = [Permission.UserAdmin]
        };

        var user = new User
        {
            Id = "user-id",
            UserName = "admin@test.com",
            NormalizedUserName = "ADMIN@TEST.COM"
        };

        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = adminRole.Id,
            User = user,
            Role = adminRole
        };

        Db.Roles.Add(adminRole);
        Db.Users.Add(user);
        Db.UserRoles.Add(userRole);
        Db.SaveChanges();

        RefreshServices();

        var crudContext = Mocker.Get<CrudContext<AppDbContext>>();
        var signInManager = Mocker.Get<SignInManager<User>>();
        var userRoleBehaviors = new UserRole.Behaviors(crudContext, signInManager);

        // Act
        var result = userRoleBehaviors.BeforeDelete(userRole);
        
        // Assert
        Assert.False(result.WasSuccessful);
        Assert.Contains("cannot remove", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("remaining user admins", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void UserRole_BeforeDelete_AllowsRemovingNonAdminUserRole()
    {
        // Arrange
        var adminRole = new Role
        {
            Id = "admin-role-id",
            Name = "Admin",
            NormalizedName = "ADMIN",
            Permissions = [Permission.UserAdmin]
        };

        var regularRole = new Role
        {
            Id = "regular-role-id",
            Name = "Regular",
            NormalizedName = "REGULAR",
            Permissions = [Permission.ViewAuditLogs]
        };

        var adminUser = new User
        {
            Id = "admin-user-id",
            UserName = "admin@test.com",
            NormalizedUserName = "ADMIN@TEST.COM"
        };

        var regularUser = new User
        {
            Id = "regular-user-id",
            UserName = "regular@test.com",
            NormalizedUserName = "REGULAR@TEST.COM"
        };

        var adminUserRole = new UserRole
        {
            UserId = adminUser.Id,
            RoleId = adminRole.Id,
            User = adminUser,
            Role = adminRole
        };

        var regularUserRole = new UserRole
        {
            UserId = regularUser.Id,
            RoleId = regularRole.Id,
            User = regularUser,
            Role = regularRole
        };

        Db.Roles.AddRange(adminRole, regularRole);
        Db.Users.AddRange(adminUser, regularUser);
        Db.UserRoles.AddRange(adminUserRole, regularUserRole);
        Db.SaveChanges();

        RefreshServices();

        var crudContext = Mocker.Get<CrudContext<AppDbContext>>();
        var signInManager = Mocker.Get<SignInManager<User>>();
        var userRoleBehaviors = new UserRole.Behaviors(crudContext, signInManager);

        // Act
        var result = userRoleBehaviors.BeforeDelete(regularUserRole);
        
        // Assert
        Assert.True(result.WasSuccessful);
    }

    [Fact]
    public void UserRole_BeforeDelete_AllowsRemovingAdminWhenOtherAdminsExist()
    {
        // Arrange
        var adminRole = new Role
        {
            Id = "admin-role-id",
            Name = "Admin",
            NormalizedName = "ADMIN",
            Permissions = [Permission.UserAdmin]
        };

        var user1 = new User
        {
            Id = "user-1",
            UserName = "admin1@test.com",
            NormalizedUserName = "ADMIN1@TEST.COM"
        };

        var user2 = new User
        {
            Id = "user-2",
            UserName = "admin2@test.com",
            NormalizedUserName = "ADMIN2@TEST.COM"
        };

        var userRole1 = new UserRole
        {
            UserId = user1.Id,
            RoleId = adminRole.Id,
            User = user1,
            Role = adminRole
        };

        var userRole2 = new UserRole
        {
            UserId = user2.Id,
            RoleId = adminRole.Id,
            User = user2,
            Role = adminRole
        };

        Db.Roles.Add(adminRole);
        Db.Users.AddRange(user1, user2);
        Db.UserRoles.AddRange(userRole1, userRole2);
        Db.SaveChanges();

        RefreshServices();

        var crudContext = Mocker.Get<CrudContext<AppDbContext>>();
        var signInManager = Mocker.Get<SignInManager<User>>();
        var userRoleBehaviors = new UserRole.Behaviors(crudContext, signInManager);

        // Act
        var result = userRoleBehaviors.BeforeDelete(userRole1);
        
        // Assert
        Assert.True(result.WasSuccessful);
    }

    [Fact]
    public void Role_BeforeSave_AllowsAddingUserAdminPermission()
    {
        // Arrange - Role without UserAdmin permission
        var regularRole = new Role
        {
            Id = "regular-role-id",
            Name = "Regular",
            NormalizedName = "REGULAR",
            Permissions = [Permission.ViewAuditLogs]
        };

        Db.Roles.Add(regularRole);
        Db.SaveChanges();

        RefreshServices();

        var roleManager = Mocker.Get<RoleManager<Role>>();
        var crudContext = Mocker.Get<CrudContext<AppDbContext>>();
        var roleBehaviors = new Role.Behaviors(roleManager, crudContext);

        // Create updated role with UserAdmin permission added
        var updatedRole = new Role
        {
            Id = regularRole.Id,
            Name = regularRole.Name,
            NormalizedName = regularRole.NormalizedName,
            Permissions = [Permission.ViewAuditLogs, Permission.UserAdmin] // Added UserAdmin
        };

        // Act
        var result = roleBehaviors.BeforeSave(SaveKind.Update, regularRole, updatedRole);
        
        // Assert - Should allow adding UserAdmin permission
        Assert.True(result.WasSuccessful);
    }

    [Fact]
    public void Role_BeforeSave_AllowsCreateOperations()
    {
        // Arrange - New role with UserAdmin permission
        var newRole = new Role
        {
            Id = "new-role-id",
            Name = "NewAdmin",
            NormalizedName = "NEWADMIN",
            Permissions = [Permission.UserAdmin]
        };

        RefreshServices();

        var roleManager = Mocker.Get<RoleManager<Role>>();
        var crudContext = Mocker.Get<CrudContext<AppDbContext>>();
        var roleBehaviors = new Role.Behaviors(roleManager, crudContext);

        // Act
        var result = roleBehaviors.BeforeSave(SaveKind.Create, null, newRole);
        
        // Assert - Should allow creating new roles
        Assert.True(result.WasSuccessful);
    }

    [Fact]
    public void UserRole_BeforeDelete_HandlesInvalidCompositeId()
    {
        // Arrange
        var adminRole = new Role
        {
            Id = "admin-role-id",
            Name = "Admin",
            NormalizedName = "ADMIN",
            Permissions = [Permission.UserAdmin]
        };

        var user = new User
        {
            Id = "user-id",
            UserName = "admin@test.com",
            NormalizedUserName = "ADMIN@TEST.COM"
        };

        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = adminRole.Id,
            User = user,
            Role = adminRole
        };

        Db.Roles.Add(adminRole);
        Db.Users.Add(user);
        Db.UserRoles.Add(userRole);
        Db.SaveChanges();

        RefreshServices();

        // Create a UserRole with malformed ID for testing edge case
        var userRoleWithInvalidId = new UserRole
        {
            UserId = "user-id",
            RoleId = "role-id",
            User = user,
            Role = new Role { Permissions = [Permission.ViewAuditLogs] } // Non-admin role
        };
        userRoleWithInvalidId.Id = "invalid-format"; // This will cause Split to return incorrect parts

        var crudContext = Mocker.Get<CrudContext<AppDbContext>>();
        var signInManager = Mocker.Get<SignInManager<User>>();
        var userRoleBehaviors = new UserRole.Behaviors(crudContext, signInManager);

        // Act
        var result = userRoleBehaviors.BeforeDelete(userRoleWithInvalidId);
        
        // Assert - Should allow deletion when ID parsing fails (graceful degradation)
        Assert.True(result.WasSuccessful);
    }

    [Fact]
    public void Role_WouldLeaveNoAdmins_ReturnsFalseWhenMultipleUsersHaveSameAdminRole()
    {
        // Arrange - Multiple users with the same admin role
        var adminRole = new Role
        {
            Id = "admin-role-id",
            Name = "Admin",
            NormalizedName = "ADMIN",
            Permissions = [Permission.UserAdmin]
        };

        var user1 = new User
        {
            Id = "user-1",
            UserName = "admin1@test.com",
            NormalizedUserName = "ADMIN1@TEST.COM"
        };

        var user2 = new User
        {
            Id = "user-2",
            UserName = "admin2@test.com",
            NormalizedUserName = "ADMIN2@TEST.COM"
        };

        var userRole1 = new UserRole
        {
            UserId = user1.Id,
            RoleId = adminRole.Id,
            User = user1,
            Role = adminRole
        };

        var userRole2 = new UserRole
        {
            UserId = user2.Id,
            RoleId = adminRole.Id,
            User = user2,
            Role = adminRole
        };

        Db.Roles.Add(adminRole);
        Db.Users.AddRange(user1, user2);
        Db.UserRoles.AddRange(userRole1, userRole2);
        Db.SaveChanges();

        RefreshServices();

        var crudContext = Mocker.Get<CrudContext<AppDbContext>>();
        var signInManager = Mocker.Get<SignInManager<User>>();
        var userRoleBehaviors = new UserRole.Behaviors(crudContext, signInManager);

        // Act - Try to remove one user from the admin role
        var result = userRoleBehaviors.BeforeDelete(userRole1);
        
        // Assert - Should allow removal since other user still has admin role
        Assert.True(result.WasSuccessful);
    }
}