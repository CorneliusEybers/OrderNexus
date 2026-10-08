
using Microsoft.EntityFrameworkCore;
using OrderNexus.Domain.Entities;

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
