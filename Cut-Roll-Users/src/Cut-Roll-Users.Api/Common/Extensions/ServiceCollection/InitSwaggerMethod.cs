namespace Cut_Roll_Users.Api.Common.Extensions.ServiceCollection;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
public static class InitSwaggerMethod
{
    public static void InitSwagger(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo()
            {
                Title = "Cut-Roll-Users Service",
                Version = "v1",
            });
            
            options.AddServer(new OpenApiServer
            {
                Url = $"{"https://cutnroll.it.com"}/api/users"
            });

            options.AddSecurityDefinition(
                name: JwtBearerDefaults.AuthenticationScheme,
                securityScheme: new OpenApiSecurityScheme()
                {
                    Description = "Input yout JWT token here:",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                });

            options.AddSecurityRequirement(
                new OpenApiSecurityRequirement() {
                {
                    new OpenApiSecurityScheme() {
                        Reference = new OpenApiReference() {
                            Id = JwtBearerDefaults.AuthenticationScheme,
                            Type = ReferenceType.SecurityScheme
                        }
                    },
                    new string[] {}
                }
                }
            );
        });
    }
}

