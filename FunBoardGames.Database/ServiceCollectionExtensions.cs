using FunBoardGames.Domain.ErrorHandling;
using FunBoardGames.Network.SignalR.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace FunBoardGames.Database
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFunBoardGamesDatabase(this IServiceCollection services, string connectionString)
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.MapEnum<BoardGameType>("game_type");
            dataSourceBuilder.MapEnum<ErrorType>("error_type");
            var dataSource = dataSourceBuilder.Build();

            services.AddDbContextFactory<FunBoardGamesDbContext>(options =>
                options.UseNpgsql(dataSource));

            services.AddScoped(sp =>
                sp.GetRequiredService<IDbContextFactory<FunBoardGamesDbContext>>().CreateDbContext());

            return services;
        }
    }
}
