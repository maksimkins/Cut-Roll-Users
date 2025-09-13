namespace Cut_Roll_Users.Api.Common.Extensions.WebApplicationBuilder;

using System;
using System.IO;
using Microsoft.AspNetCore.Builder;

public static class SetupVariablesMethod
{
    public static void SetupVariables(this WebApplicationBuilder builder)
    {
        
        var postgresConnectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING") ?? throw new SystemException("there is no var POSTGRES_CONNECTION_STRING");

        var jwt_key = Environment.GetEnvironmentVariable("JWT_KEY") ?? throw new SystemException("there is no var JWT_KEY");
        var jwt_life_time = Environment.GetEnvironmentVariable("JWT_LIFE_TIME_IN_MINUTES") ?? throw new SystemException("there is no var JWT_LIFE_TIME_IN_MINUTES");
        var jwt_issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? throw new SystemException("there is no var JWT_ISSUER");
        var jwt_audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? throw new SystemException("there is no var JWT_AUDIENCE");

        var rabbit_mq_hostname = Environment.GetEnvironmentVariable("RABBIT_MQ_HOSTNAME") ?? throw new SystemException("there is no var RABBIT_MQ_HOSTNAME");
        var rabbit_mq_username = Environment.GetEnvironmentVariable("RABBIT_MQ_USERNAME") ?? throw new SystemException("there is no var RABBIT_MQ_USERNAME");
        var rabbit_mq_password = Environment.GetEnvironmentVariable("RABBIT_MQ_PASSWORD") ?? throw new SystemException("there is no var RABBIT_MQ_PASSWORD");


        var pinecone_api_key = Environment.GetEnvironmentVariable("PINECONE_API_KEY") ?? throw new SystemException("there is no var PINECONE_API_KEY");
        var pinecone_environment = Environment.GetEnvironmentVariable("PINECONE_ENVIRONMENT") ?? throw new SystemException("there is no var PINECONE_ENVIRONMENT");
        var pinecone_index_name = Environment.GetEnvironmentVariable("PINECONE_INDEX_NAME") ?? throw new SystemException("there is no PINECONE_INDEX_NAME");
        var pinecone_vector_dimensions = Environment.GetEnvironmentVariable("PINECONE_VECTOR_DIMENSIONS") ?? throw new SystemException("there is no PINECONE_VECTOR_DIMENSIONS");

        var embedding_model_path = Environment.GetEnvironmentVariable("EMBEDDING_MODEL_PATH") ?? throw new SystemException("there is no var EMBEDDING_MODEL_PATH");


        builder.Configuration["Jwt:Key"] = jwt_key;
        builder.Configuration["Jwt:LifeTimeInMinutes"] = jwt_life_time;
        builder.Configuration["Jwt:Issuer"] = jwt_issuer;
        builder.Configuration["Jwt:Audience"] = jwt_audience;

        builder.Configuration["ConnectionStrings:SqlConnection"] = postgresConnectionString;

        builder.Configuration["RabbitMq:HostName"] = rabbit_mq_hostname;
        builder.Configuration["RabbitMq:UserName"] = rabbit_mq_username;
        builder.Configuration["RabbitMq:Password"] = rabbit_mq_password;

        builder.Configuration["Pinecone:ApiKey"] = pinecone_api_key;
        builder.Configuration["Pinecone:Environment"] = pinecone_environment;
        builder.Configuration["Pinecone:IndexName"] = pinecone_index_name;
        builder.Configuration["Pinecone:VectorDimensions"] = pinecone_vector_dimensions;

        builder.Configuration["Embedding:ModelPath"] = Path.Combine(embedding_model_path, "model.onnx");
        builder.Configuration["Embedding:BatchSize"] = "100";
        builder.Configuration["Embedding:MaxRetries"] = "3";

        builder.Configuration["BackgroundServices:EmbeddingProcessingInterval"] = "00:05:00";
        builder.Configuration["BackgroundServices:MaxConcurrentBatches"] = "2";
    }
}