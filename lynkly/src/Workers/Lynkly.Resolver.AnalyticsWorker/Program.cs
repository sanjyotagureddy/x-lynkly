using lynkly.ServiceDefaults;

using Lynkly.Resolver.Infrastructure.Messaging.DependencyInjection;
using Lynkly.Resolver.AnalyticsWorker;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddResolverMessaging(builder.Configuration);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
