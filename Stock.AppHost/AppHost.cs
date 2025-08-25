var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("Redis");
var postgres = builder.AddPostgres("Postgres");
var stockDb = postgres.AddDatabase("StockDb");

builder.AddProject<Projects.Stock_RealTime_Api>("stock-realtime-api")
    .WithReference(redis)
    .WithReference(stockDb)
    .WithEndpoint();

builder.AddContainer("pgadmin", "dpage/pgadmin4")
    .WithEnvironment("PGADMIN_DEFAULT_EMAIL", "admin@example.com")
    .WithEnvironment("PGADMIN_DEFAULT_PASSWORD", "admin")
    .WithEndpoint(port: 65003, targetPort: 80)
    .WithReference(postgres);

builder.AddContainer("redisinsight", "redis/redisinsight:latest")
    .WithEndpoint(port: 65004, targetPort: 5540)
    .WithReference(redis);

builder.Build().Run();
