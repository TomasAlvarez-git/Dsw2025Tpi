using Dsw2025Tpi.Application.Helpers;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Data.Repositories;
using Dsw2025Tpi.Domain.Interfaces;

namespace Dsw2025Tpi.Api.Helpers
{
    public static class AplicationServicesExtensions
    {
        public static IServiceCollection AddAplicationServices(this IServiceCollection services)
        {
            // Servicio que genera los tokens JWT
            services.AddSingleton<JwtTokenService>();
            services.AddScoped<IRepository, EfRepository>(); // Patrón repositorio
            services.AddTransient<IProductsManagementService, ProductsManagementService>();
            services.AddTransient<ProductsManagmentServiceExtensions>();
            services.AddTransient<IOrdersManagementService, OrdersManagementService>();
            services.AddTransient<OrdersManagementServiceExtensions>();
            return services;
        }
    }
}
