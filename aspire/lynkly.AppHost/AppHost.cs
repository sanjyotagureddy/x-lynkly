var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("lynkly-postgres")
  .WithDataVolume()
  .WithHostPort(5432); 
var database = postgres.AddDatabase("lynkly-db");

var redis = builder.AddRedis("lynkly-redis").WithDataVolume();

builder.AddProject<Projects.Lynkly_Resolver_API>("lynkly-resolver-api")
    .WithReference(database)
    .WithReference(redis)
    .WaitFor(database)
    .WaitFor(redis);

builder.AddProject<Projects.Lynkly_Resolver_AnalyticsWorker>("lynkly-resolver-analytics-worker");

builder.Build().Run();
