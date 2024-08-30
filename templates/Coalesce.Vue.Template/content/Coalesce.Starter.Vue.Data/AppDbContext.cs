using Microsoft.EntityFrameworkCore;
using Coalesce.Starter.Vue.Data.Models;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using IntelliTect.Coalesce.AuditLogging;
using Coalesce.Starter.Vue.Data.Coalesce;

namespace Coalesce.Starter.Vue.Data;

[Coalesce]
public class AppDbContext
	: IdentityDbContext<
		AppUser,
		AppRole,
		string,
		IdentityUserClaim<string>,
		AppUserRole,
		IdentityUserLogin<string>,
		AppRoleClaim,
		IdentityUserToken<string>
	>, 
	IDataProtectionKeyContext, 
	IAuditLogDbContext<AuditLog>
{
	public bool SuppressAudit { get; set; } = false;
	
	public AppDbContext() { }

	public AppDbContext(DbContextOptions options) : base(options) { }

    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<AuditLogProperty> AuditLogProperties { get; set; }

    [InternalUse]
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
        .UseStamping<TrackingBase>((entity, user) => entity.SetTracking(user))
        .UseCoalesceAuditLogging<AuditLog>(x => x
            .WithAugmentation<AuditOperationContext>()
            .ConfigureAudit(config =>
            {
                static string ShaString(byte[]? bytes) => bytes is null ? "" : Convert.ToBase64String(SHA1.HashData(bytes));

                config
                    .FormatType<byte[]>(ShaString)
                    .Exclude<DataProtectionKey>()
                    .ExcludeProperty<TrackingBase>(x => new { x.CreatedBy, x.CreatedById, x.CreatedOn, x.ModifiedBy, x.ModifiedById, x.ModifiedOn });
            })
        );
    }

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		// Remove cascading deletes.
		foreach (var relationship in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
		{
			relationship.DeleteBehavior = DeleteBehavior.Restrict;
		}

        builder.Entity<AppUserRole>(userRole =>
        {
            userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

            userRole.HasOne(ur => ur.Role)
                .WithMany()
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            userRole.HasOne(ur => ur.User)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AppRoleClaim>(e =>
        {
            e.HasOne(ur => ur.Role)
                .WithMany(x => x.RoleClaims)
                .HasForeignKey(x => x.RoleId)
                .HasPrincipalKey(x => x.Id)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });
	}
}
