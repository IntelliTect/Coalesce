using Coalesce.Starter.Vue.Data.Coalesce;
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
        RoleClaim,
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

#if Tenancy
    private string? _TenantId;

    /// <summary>
    /// The tenant ID used to filter results and assign new objects to a tenant.
    /// </summary>
    public string? TenantId
    {
        get => _TenantId;
        set
        {
            if (_TenantId != null && value != _TenantId && ChangeTracker.Entries().Any())
            {
                throw new InvalidOperationException("Cannot change the TenantId of an active DbContext. Make a new one through IDbContextFactory to perform operations on different tenants, or call ForceSetTenant().");
            }
            _TenantId = value;
        }
    }

    public string TenantIdOrThrow => TenantId ?? throw new InvalidOperationException("TenantId not set on AppDbContext");

    /// <summary>
    /// Resets the <see cref="DbContext"/>'s change tracker and switches the current tenant to <paramref name="tenantId"/>.
    /// </summary>
    public void ForceSetTenant(string tenantId)
    {
        if (TenantId != tenantId)
        {
            ChangeTracker.Clear();
            TenantId = tenantId;
        }
    }
#endif

    public AppDbContext() { }

    public AppDbContext(DbContextOptions options) : base(options) { }

#if AuditLogs
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AuditLogProperty> AuditLogProperties => Set<AuditLogProperty>();
#endif

#if Tenancy
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantMembership> TenantMemberships => Set<TenantMembership>();
#endif

#if (Identity && UserPictures)
    public DbSet<UserPhoto> UserPhotos => Set<UserPhoto>();
#endif

#if ExampleModel
    public DbSet<Widget> Widgets => Set<Widget>();
#endif

    [InternalUse]
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

#if (TrackingBase || AuditLogs || Tenancy)
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
#if TrackingBase
        .UseStamping<TrackingBase>((entity, user) => entity.SetTracking(user))
#endif
#if Tenancy
        .UseCoalesceMultiTenancy<ITenanted>(t => t.TenantId, () => TenantIdOrThrow)
#endif
#if AuditLogs
        .UseCoalesceAuditLogging<AuditLog>(x => x
            .WithAugmentation<AuditOperationContext>()
            .ConfigureAudit(config =>
            {
                static string ShaString(byte[]? bytes) => bytes is null ? "" : "SHA1:" + Convert.ToBase64String(SHA1.HashData(bytes));

                config
                    .FormatType<byte[]>(ShaString)
                    .Exclude<DataProtectionKey>()
#if TrackingBase
                    .ExcludeProperty<TrackingBase>(x => new { x.CreatedById, x.CreatedOn, x.ModifiedById, x.ModifiedOn })
#endif
#if Identity
                    .Format<User>(x => x.PasswordHash, x => "<password changed/rehashed>")
                    .Format<User>(x => x.SecurityStamp, x => "<stamp changed>")
                    .Exclude<IdentityUserToken<string>>()
                    .ExcludeProperty<User>(x => new { x.ConcurrencyStamp })
#endif
#if Tenancy
                    .ExcludeProperty<ITenanted>(x => new { x.TenantId })
#endif
                ;
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
        HashSet<Type> allowedCascadePrincipals = [];
        HashSet<Type> allowedCascadeDependents = [
//#if Passkeys
            typeof(IdentityPasskeyData)
//#endif
        ];

        foreach (var relationship in builder.Model
            .GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys())
            .Where(fk => !allowedCascadePrincipals.Contains(fk.PrincipalEntityType.ClrType)
                && !allowedCascadeDependents.Contains(fk.DeclaringEntityType.ClrType)))
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

        builder.Entity<User>(e =>
        {
            e.Property(e => e.Id).HasMaxLength(36);
        });

        builder.Entity<Role>(e =>
        {
            e.Property(e => e.Id).HasMaxLength(36);
            e.PrimitiveCollection(e => e.Permissions);

#if Tenancy
            // Fix index that doesn't account for tenanted roles
            e.Metadata.RemoveIndex(e.Metadata.GetIndexes().Where(i => i.Properties[0].Name == nameof(Role.NormalizedName)).Single());
            e.HasIndex(r => new { r.TenantId, r.NormalizedName }).IsUnique();

#endif
            e.HasMany<RoleClaim>()
                .WithOne(rc => rc.Role)
                .HasPrincipalKey(r => r.Id)
                .HasForeignKey(rc => rc.RoleId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });
#endif

    }

}
