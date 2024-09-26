using Coalesce.Starter.Vue.Data.Coalesce;
#if AuditLogs
using IntelliTect.Coalesce.AuditLogging;
#endif
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using System.Linq.Expressions;
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
                throw new InvalidOperationException("Cannot change the TenantId of an active DbContext. Make a new one through DbContextFactory to perform operations on different tenants, or call ForceSetTenant().");
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

#if UserPictures
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
        .AddInterceptors(new TenantInterceptor())
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
                    .ExcludeProperty<TrackingBase>(x => new { x.CreatedBy, x.CreatedById, x.CreatedOn, x.ModifiedBy, x.ModifiedById, x.ModifiedOn })
#endif
#if Identity
                    .ExcludeProperty<User>(x => new { x.PasswordHash })
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

#if Tenancy
        // Setup tenancy model configuration. This should be after all other model configuration.
        foreach (var model in builder.Model
            .GetEntityTypes()
            .Where(e => e.ClrType.GetInterface(nameof(ITenanted)) != null)
            .ToList())
        {
            // Create the global query filter for the model that will restrict data to the current tenant.
            var param = Expression.Parameter(model.ClrType);
            model.SetQueryFilter(Expression.Lambda(
                Expression.Equal(
                    Expression.MakeMemberAccess(param, model.ClrType.GetProperty("TenantId")!),
                    Expression.MakeMemberAccess(Expression.Constant(this), this.GetType().GetProperty("TenantIdOrThrow")!)
                ),
                param
            ));

            // Put the tenantID as the first part of each tenanted entity's PK.

            // This is done in a way that is transparent to Coalesce since Coalesce
            // and APIs are essentially unconcerned with tenancy - the tenant is always derived
            // from the logged in user. Also because Coalesce doesn't support composite keys.

            // Doing this lets us include tenantIDs as part of foreign keys,
            // and also affords us slightly more performance when doing joins
            // since data from each tenant will be clustered together.

            var key = model.FindPrimaryKey();
            var tenantIdProp = model.FindProperty(nameof(ITenanted.TenantId));
            if (key is { Properties.Count: 1 } && tenantIdProp is not null && key.Properties.Single() != tenantIdProp)
            {
                // A value generator is added so that entities can be .Add()ed to the DbContext
                // while their TenantID is still null (if EF can't figure out the TenantId through any other navigation prop).
                tenantIdProp.SetValueGeneratorFactory((p, t) => new TenantIdValueGenerator());

                var pkProp = key.Properties.Single();
                var oldPkGenerated = pkProp.ValueGenerated;

                if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
                {
                    // Unfortunately for Sqlite and unit testing, we can't make the tenant part of the PK.
                    // See https://stackoverflow.com/questions/49592274/how-to-create-autoincrement-column-in-sqlite-using-ef-core
                    // So, do the next best thing and add a second FK to all relationships that includes the tenantID.

                    var tenantedAk = model.AddKey(new[] { tenantIdProp, pkProp })!;

                    foreach (var fk in model.GetReferencingForeignKeys().ToList())
                    {
                        var dependentTenantId = fk.DeclaringEntityType.FindProperty(nameof(ITenanted.TenantId));
                        if (dependentTenantId is null) continue;

                        var newFk = fk.DeclaringEntityType.AddForeignKey(new[] {
                            dependentTenantId,
                            fk.Properties.Single()
                        }, tenantedAk, model);

                        newFk.DeleteBehavior = DeleteBehavior.NoAction;
                    }
                }
                else
                {
                    // SQL Server:

                    // TenantID goes first, for clustering.
                    var newPk = model.SetPrimaryKey(new[] { tenantIdProp, pkProp })!;

                    foreach (var fk in model.GetReferencingForeignKeys().ToList())
                    {
                        fk.SetProperties(new[] {
                            fk.DeclaringEntityType.FindProperty(nameof(ITenanted.TenantId))
                                ?? throw new InvalidOperationException($"Foreign key from untenanted entity {fk.DeclaringEntityType} cannot reference tenanted principal {model}"),
                            fk.Properties.Single()
                        }, newPk);
                    }

                    // Keep the old PK prop as an identity column if it previously was before we changed the PK.
                    pkProp.ValueGenerated = oldPkGenerated;
                }
            }
        }
#endif
    }

#if Tenancy
    class TenantIdValueGenerator : ValueGenerator<string>
    {
        public override bool GeneratesTemporaryValues => false;

        public override string Next(EntityEntry entry) => ((AppDbContext)entry.Context).TenantIdOrThrow;
    }

    class TenantInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            var db = (AppDbContext)eventData.Context!;
            foreach (var entry in db.ChangeTracker.Entries<ITenanted>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property(nameof(ITenanted.TenantId)).CurrentValue = db.TenantId;
                }
                else if (entry.State == EntityState.Modified && entry.Property(nameof(ITenanted.TenantId)).IsModified)
                {
                    throw new InvalidOperationException("Cannot change the TenantId of an existing entity.");
                }
            }

            return result;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            return new ValueTask<InterceptionResult<int>>(SavingChanges(eventData, result));
        }
    }
#endif
}
