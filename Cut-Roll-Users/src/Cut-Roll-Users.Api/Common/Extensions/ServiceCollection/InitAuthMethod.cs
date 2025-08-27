

namespace Cut_Roll_Users.Api.Common.Extensions.ServiceCollection;

using System.Security.Claims;
using Cut_Roll_Users.Core.Common.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

public static class InitAuthMethod
{
    public static void InitAuth(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        serviceCollection.Configure<JwtOptions>(jwtSection);
        serviceCollection
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var jwtOptions = jwtSection.Get<JwtOptions>() ?? throw new Exception("cannot find Jwt Section");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,

                    ValidateLifetime = true,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(jwtOptions!.KeyInBytes)
                };
            });

        serviceCollection.AddAuthorization(options =>
        {
            options.AddPolicy("Essentials", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("EmailConfirmed", "True");
                policy.RequireClaim(ClaimTypes.Email);
                policy.RequireClaim(ClaimTypes.NameIdentifier);
            });

            options.AddPolicy("NotMuted", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("IsMuted", "False");
            });
            
            options.AddPolicy("AdminOnly", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("Admin"); 
            });
        });
    }
}