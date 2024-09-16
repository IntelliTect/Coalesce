using Coalesce.Starter.Vue.Data.Coalesce;
using Coalesce.Starter.Vue.Data.Models;
#if AuditLogs
using IntelliTect.Coalesce.AuditLogging;
#endif
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Coalesce.Starter.Vue.Data;

[Coalesce]
public class AppDbContext
#if Identity
	: IdentityDbContext<
		User,
		Role,
		string,
		IdentityUserClaim<string>,
		UserRole,
		IdentityUserLogin<string>,
		IdentityRoleClaim<string>,
		IdentityUserToken<string>
	>
#else
	: DbContext
#endif
	, IDataProtectionKeyContext
#if AuditLogs
	, IAuditLogDbContext<AuditLog>
#endif
{
	public bool SuppressAudit { get; set; } = false;
	
	public AppDbContext() { }

	public AppDbContext(DbContextOptions options) : base(options) { }

#if UserPictures
	public DbSet<UserPhoto> UserPhotos { get; set; }
#endif

#if AuditLogs
	public DbSet<AuditLog> AuditLogs { get; set; }
	public DbSet<AuditLogProperty> AuditLogProperties { get; set; }
#endif

	public DbSet<Widget> Widgets { get; set; }

	[InternalUse]
	public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

#if (TrackingBase || AuditLogs)
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder
#if TrackingBase
		.UseStamping<TrackingBase>((entity, user) => entity.SetTracking(user))
#endif
#if AuditLogs
		.UseCoalesceAuditLogging<AuditLog>(x => x
			.WithAugmentation<AuditOperationContext>()
			.ConfigureAudit(config =>
			{
				static string ShaString(byte[]? bytes) => bytes is null ? "" : Convert.ToBase64String(SHA1.HashData(bytes));

				config
					.FormatType<byte[]>(ShaString)
					.Exclude<DataProtectionKey>()
#if TrackingBase
					.ExcludeProperty<TrackingBase>(x => new { x.CreatedBy, x.CreatedById, x.CreatedOn, x.ModifiedBy, x.ModifiedById, x.ModifiedOn });
#else
					;
#endif
			})
		)
#endif
		;
	}
#endif

    protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		// Remove cascading deletes.
		foreach (var relationship in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
		{
			relationship.DeleteBehavior = DeleteBehavior.Restrict;
		}

#if Identity
		builder.Entity<UserRole>(userRole =>
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

        builder.Entity<Role>(e =>
        {
            e.PrimitiveCollection(e => e.Permissions).ElementType().HasConversion<string>();
        });
#endif
	}
}
