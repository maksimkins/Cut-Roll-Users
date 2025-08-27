namespace Cut_Roll_Users.Api.Common.Extensions.WebApplicationBuilder;

using Cut_Roll_Users.Core.Common.Options;
using Microsoft.AspNetCore.Builder;

public static class InitMessageBrokerMethod
{
    public static void InitMessageBroker(this WebApplicationBuilder builder)
    {
        var rabbitMqSection = builder.Configuration.GetSection("RabbitMq");
        builder.Services.Configure<RabbitMqOptions>(rabbitMqSection);
    }
}