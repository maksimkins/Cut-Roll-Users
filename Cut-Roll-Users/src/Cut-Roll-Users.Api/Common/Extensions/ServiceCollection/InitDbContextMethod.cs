using Cut_Roll_Users.Infrastructure.Common.Data;
using Microsoft.EntityFrameworkCore;

namespace Cut_Roll_Users.Api.Common.Extensions.ServiceCollection;
public static class InitDbContextMethod
{
    public static void InitDbContext(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqlConnection")
            ?? throw new SystemException("connectionString is not set");

        serviceCollection.AddDbContext<UsersDbContext>(options =>
            options.UseNpgsql(connectionString));
    }
}