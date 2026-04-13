using lynkly.ServiceDefaults;

using Lynkly.Resolver.AnalyticsWorker;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
