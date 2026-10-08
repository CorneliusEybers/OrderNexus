
using Microsoft.EntityFrameworkCore;
using OrderNexus.Domain.Entities;

namespace OrderNexus.Infrastructure.Persistence.Seeding
{
    /// <summary>
    /// Seeds the OrderNexus database with initial lookup data.
    /// </summary>
    public static class DatabaseSeeder
    {
        #region Class Variables

        private static readonly DateTime seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private const string SeedUser = "System";

        #endregion

        #region Public Methods

        /// <summary>
        /// Populates empty lookup tables with initial data.
        /// This operation is safe to repeat.
        /// </summary>
        public static async Task SeedAsync(OrderNexusDbContext context,
                                           CancellationToken cancellationToken = default)
        {
            await SeedCustomersAsync(context, cancellationToken);

            await SeedSalesRepsAsync(context, cancellationToken);

            await SeedCurrenciesAsync(context, cancellationToken);

            await SeedOrderStatusesAsync(context, cancellationToken);

            await SeedItemsAsync(context, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Seeds sample customers for the order-entry interface.
        /// </summary>
        private static async Task SeedCustomersAsync(OrderNexusDbContext context, CancellationToken cancellationToken)
        {
            if (await context.Customers.AnyAsync(cancellationToken))
            {
                return;
            }

            context.Customers.AddRange(
                new Customer
                {
                    Id = 1,
                    Name = "Acme Industries",
                    CreatedBy = SeedUser,
                    CreatedDateTime = seedDate
                },
                new Customer
                {
                    Id = 2,
                    Name = "BlueWave Technologies",
                    CreatedBy = SeedUser,
                    CreatedDateTime = seedDate
                },
                new Customer
                {
                    Id = 3,
                    Name = "Summit Logistics",
                    CreatedBy = SeedUser,
                    CreatedDateTime = seedDate
                },
                new Customer
                {
                    Id = 4,
                    Name = "Greenfield Supplies",
                    CreatedBy = SeedUser,
                    CreatedDateTime = seedDate
                },
                new Customer
                {
                    Id = 5,
                    Name = "Horizon Trading",
                    CreatedBy = SeedUser,
                    CreatedDateTime = seedDate
                });
        }

        /// <summary>
        /// Seeds sales representatives.
        /// </summary>
        private static async Task SeedSalesRepsAsync(OrderNexusDbContext context, CancellationToken cancellationToken)
        {
            if (await context.SalesReps.AnyAsync(cancellationToken))
            {
                return;
            }

            context.SalesReps.AddRange(
                new SalesRep
                {
                    Id = 1,
                    Name = "James Mitchell",
                    Email = "james.mitchell@example.com",
                    CreatedBy = SeedUser,
                    CreatedDateTime = seedDate
                },
                new SalesRep
                {
                    Id = 2,
                    Name = "Sarah Williams",
                    Email = "sarah.williams@example.com",
                    CreatedBy = SeedUser,
                    CreatedDateTime = seedDate
                },
                new SalesRep
                {
                    Id = 3,
                    Name = "Daniel Roberts",
                    Email = "daniel.roberts@example.com",
                    CreatedBy = SeedUser,
                    CreatedDateTime = seedDate
                });
        }

        /// <summary>
        /// Seeds the currency supported by the initial catalogue.
        /// </summary>
        private static async Task SeedCurrenciesAsync(OrderNexusDbContext context, CancellationToken cancellationToken)
        {
            if (await context.Currencies.AnyAsync(cancellationToken))
            {
                return;
            }

            context.Currencies.Add(
                new Currency
                {
                    Id = 1,
                    ISOCode = "ZAR",
                    CurrencyName = "South African Rand",
                    CreatedBy = SeedUser,
                    CreatedDateTime = seedDate
                });
        }

        /// <summary>
        /// Seeds the supported order lifecycle statuses.
        /// </summary>
        private static async Task SeedOrderStatusesAsync(OrderNexusDbContext context, CancellationToken cancellationToken)
        {
            if (await context.OrderStatuses.AnyAsync(cancellationToken))
            {
                return;
            }

            context.OrderStatuses.AddRange(
                new OrderStatus
                {
                    Id = 1,
                    Code = "PENDING",
                    Name = "Pending",
                    Description = "Order submitted and awaiting confirmation.",
                    CreatedBy = SeedUser,
                    CreatedDateTime = seedDate
                },
                new OrderStatus
                {
                    Id = 2,
                    Code = "CONFIRMED",
                    Name = "Confirmed",
                    Description = "Order accepted for fulfilment.",
                    CreatedBy = SeedUser,
                    CreatedDateTime = seedDate
                },
                new OrderStatus
                {
                    Id = 3,
                    Code = "FULFILLED",
                    Name = "Fulfilled",
                    Description = "Order fulfilment completed.",
                    CreatedBy = SeedUser,
                    CreatedDateTime = seedDate
                },
                new OrderStatus
                {
                    Id = 4,
                    Code = "CANCELLED",
                    Name = "Cancelled",
                    Description = "Order cancelled before fulfilment.",
                    CreatedBy = SeedUser,
                    CreatedDateTime = seedDate
                });
        }

        /// <summary>
        /// Seeds five products for the Angular item selector.
        /// Prices are expressed in ZAR.
        /// </summary>
        private static async Task SeedItemsAsync(OrderNexusDbContext context, CancellationToken cancellationToken)
        {
            if (await context.Items.AnyAsync(cancellationToken))
            {
                return;
            }

            context.Items.AddRange(
                new Item
                {
                    Id = 1,
                    SKU = "LS-001",
                    Name = "Laptop Stand",
                    UnitPrice = 450.00m,
                    CreatedBy = SeedUser,
                    CreatedDateTime = seedDate
                },
                new Item
                {
                    Id = 2,
                    SKU = "WM-002",
                    Name = "Wireless Mouse",
                    UnitPrice = 275.50m,
                    CreatedBy = SeedUser,
                    CreatedDateTime = seedDate
                },
                new Item
                {
                    Id = 3,
                    SKU = "UC-003",
                    Name = "USB-C Hub",
                    UnitPrice = 650.00m,
                    CreatedBy = SeedUser,
                    CreatedDateTime = seedDate
                },
                new Item
                {
                    Id = 4,
                    SKU = "KB-004",
                    Name = "Mechanical Keyboard",
                    UnitPrice = 850.75m,
                    CreatedBy = SeedUser,
                    CreatedDateTime = seedDate
                },
                new Item
                {
                    Id = 5,
                    SKU = "MN-005",
                    Name = "24-inch Monitor",
                    UnitPrice = 3200.00m,
                    CreatedBy = SeedUser,
                    CreatedDateTime = seedDate
                });
        }

        #endregion
    }
}
