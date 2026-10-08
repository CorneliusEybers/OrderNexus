
using Microsoft.EntityFrameworkCore;
using OrderNexus.Domain.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OrderNexus.Domain.Common;

namespace OrderNexus.Infrastructure.Persistence
{
    /// <summary>
    /// Represents the database context for the OrderNexus application.
    /// </summary>
    public class OrderNexusDbContext : DbContext
    {
        #region Constructor

        public OrderNexusDbContext(DbContextOptions<OrderNexusDbContext> options) : base(options)
        {
        }

        #endregion

        #region Public Properties

        public DbSet<Customer> Customers => Set<Customer>();

        public DbSet<SalesRep> SalesReps => Set<SalesRep>();

        public DbSet<Item> Items => Set<Item>();

        public DbSet<Currency> Currencies => Set<Currency>();

        public DbSet<OrderStatus> OrderStatuses => Set<OrderStatus>();

        public DbSet<Order> Orders => Set<Order>();

        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        #endregion

        #region Public Methods

        /// <summary>
        /// Saves changes while maintaining entity audit timestamps.
        /// </summary>
        public override int SaveChanges()
        {
            ApplyAuditTimestamps();

            return base.SaveChanges();
        }

        /// <summary>
        /// Asynchronously saves changes while maintaining
        /// entity audit timestamps.
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditTimestamps();

            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Saves changes using the specified accept-all-changes behaviour.
        /// </summary>
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ApplyAuditTimestamps();

            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        /// <summary>
        /// Asynchronously saves changes using the specified
        /// accept-all-changes behaviour.
        /// </summary>
        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            ApplyAuditTimestamps();

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Applies UTC timestamps to newly created and
        /// modified entities.
        /// </summary>
        private void ApplyAuditTimestamps()
        {
            DateTime utcNow = DateTime.UtcNow;

            IEnumerable<EntityEntry<BaseEntity>> entries = ChangeTracker.Entries<BaseEntity>();

            foreach (EntityEntry<BaseEntity> entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:

                        if (entry.Entity.CreatedDateTime == default)
                        {
                            entry.Entity.CreatedDateTime = utcNow;
                        }

                        break;

                    case EntityState.Modified:

                        entry.Property(ent => ent.CreatedDateTime).IsModified = false;

                        entry.Property(ent => ent.CreatedBy).IsModified = false;

                        entry.Entity.UpdatedDateTime = utcNow;

                        break;
                }
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Configures entity mappings and relationships.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Automatically discover IEntityTypeConfiguration<T>
            // implementations in the Infrastructure assembly.
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderNexusDbContext).Assembly);
        }

        #endregion
    }
}
