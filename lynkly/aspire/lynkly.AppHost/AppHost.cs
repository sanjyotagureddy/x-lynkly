var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("lynkly-postgres")
  .WithDataVolume()
  .WithHostPort(5432); 
var database = postgres.AddDatabase("lynkly-db");

var redis = builder.AddRedis("lynkly-redis").WithDataVolume();
var rabbitMq = builder.AddRabbitMQ("lynkly-rabbitmq")
    .WithDataVolume()
    .WithManagementPlugin();

builder.AddProject<Projects.Lynkly_Resolver_API>("lynkly-resolver-api")
    .WithReference(database)
    .WithReference(redis)
    .WithReference(rabbitMq)
    .WaitFor(database)
    .WaitFor(redis)
    .WaitFor(rabbitMq);

builder.AddProject<Projects.Lynkly_Resolver_AnalyticsWorker>("lynkly-resolver-analytics-worker")
    .WithReference(database)
    .WithReference(rabbitMq)
    .WaitFor(database)
    .WaitFor(rabbitMq);

builder.Build().Run();
