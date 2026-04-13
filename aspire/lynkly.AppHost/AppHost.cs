var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Lynkly_Resolver_API>("lynkly-resolver-api");
builder.AddProject<Projects.Lynkly_Resolver_AnalyticsWorker>("lynkly-resolver-analytics-worker");

builder.Build().Run();
