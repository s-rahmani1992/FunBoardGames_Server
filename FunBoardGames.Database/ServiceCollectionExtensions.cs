using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FunBoardGames.Database
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers <see cref="FunBoardGamesDbContext"/> configured for PostgreSQL using the
        /// supplied connection string.
        /// </summary>
        public static IServiceCollection AddFunBoardGamesDatabase(this IServiceCollection services, string connectionString)
        {
            services.AddDbContextFactory<FunBoardGamesDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddScoped(sp =>
                sp.GetRequiredService<IDbContextFactory<FunBoardGamesDbContext>>().CreateDbContext());

            return services;
        }
    }
}
