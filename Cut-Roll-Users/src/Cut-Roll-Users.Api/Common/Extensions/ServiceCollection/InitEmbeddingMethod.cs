namespace Cut_Roll_Users.Api.Common.Extensions.ServiceCollection;

using Cut_Roll_Users.Core.Common.Options;
using Cut_Roll_Users.Core.Common.VectorDatabases.Options;
using Cut_Roll_Users.Infrastructure.Common.Options;

public static class InitEmbeddingMethod
{
    public static void InitEmbedding(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        // Configure Local Embedding Options
        var embeddingSection = configuration.GetSection("Embedding");
        serviceCollection.Configure<LocalEmbeddingOptions>(embeddingSection);

        // Configure Pinecone Options
        var pineconeSection = configuration.GetSection("Pinecone");
        serviceCollection.Configure<PineconeOptions>(pineconeSection);

        // Configure Background Service Options
        var backgroundSection = configuration.GetSection("BackgroundServices");
        serviceCollection.Configure<BackgroundServiceOptions>(backgroundSection);
    }
}
