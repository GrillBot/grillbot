using GrillBot.Database.Services;
using GrillBot.Database.Services.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;

namespace GrillBot.Database
{
    static public class DatabaseExtensions
    {
        static public IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
        {
            services
                .AddDbContext<GrillBotContext>(b => b.UseNpgsql(connectionString));

            var repositories = Assembly.GetExecutingAssembly().GetTypes()
                .Where(o => typeof(RepositoryBase).IsAssignableFrom(o))
                .ToList();

            repositories.ForEach(repo => services.AddScoped(repo));
            return services;
        }
    }
}
