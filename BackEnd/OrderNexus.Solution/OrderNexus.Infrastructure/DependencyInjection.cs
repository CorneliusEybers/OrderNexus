
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderNexus.Application.Interfaces;
using OrderNexus.Infrastructure.Persistence;
using OrderNexus.Infrastructure.Repositories;

namespace OrderNexus.Infrastructure
{
    /// <summary>
    /// Registers Infrastructure services with the
    /// application's dependency injection container.
    /// </summary>
    public static class DependencyInjection
    {
        #region Public Methods

        /// <summary>
        /// Registers the SQLite database context.
        /// </summary>
        /// <param name="services">
        /// Application service collection.
        /// </param>
        /// <param name="connectionString">
        /// SQLite database connection string.
        /// </param>
        /// <returns>
        /// The configured service collection.
        /// </returns>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<OrderNexusDbContext>(options => options.UseSqlite(connectionString));
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<ILookupRepository, LookupRepository>();

            return services;
        }

        #endregion
    }
}
