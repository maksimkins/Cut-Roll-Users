namespace Cut_Roll_Users.Api.Common.Extensions.ServiceCollection;

using Microsoft.AspNetCore.Cors.Infrastructure;

public static class InitCorsMethod
{
    public static void InitCors(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddCors(delegate (CorsOptions options)
        {
            options.AddPolicy("AllowAllOrigins", delegate (CorsPolicyBuilder builder)
            {
                builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            });
        });
    }
}