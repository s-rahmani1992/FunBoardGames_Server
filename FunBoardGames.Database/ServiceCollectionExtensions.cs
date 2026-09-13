using FunBoardGames.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

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
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.MapEnum<GameType>("game_type");
            var dataSource = dataSourceBuilder.Build();

            services.AddDbContextFactory<FunBoardGamesDbContext>(options =>
                options.UseNpgsql(dataSource));

            services.AddScoped(sp =>
                sp.GetRequiredService<IDbContextFactory<FunBoardGamesDbContext>>().CreateDbContext());

            return services;
        }
    }
}
