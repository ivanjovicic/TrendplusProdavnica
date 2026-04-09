var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis");

var openSearch = builder.AddContainer("opensearch", "opensearchproject/opensearch", "2.15.0")
    .WithEndpoint(name: "http", port: 9200, targetPort: 9200)
    .WithEnvironment("discovery.type", "single-node")
    .WithEnvironment("plugins.security.disabled", "true")
    .WithEnvironment("DISABLE_SECURITY_PLUGIN", "true")
    .WithEnvironment("OPENSEARCH_INITIAL_ADMIN_PASSWORD", "Admin123!");

builder.AddProject<Projects.TrendplusProdavnica_Api>("api")
    .WithReference(redis)
    .WithEnvironment("OpenSearch__Uri", "http://localhost:9200");

builder.Build().Run();
