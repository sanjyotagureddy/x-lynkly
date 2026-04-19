using lynkly.ServiceDefaults;
using Lynkly.Resolver.API.Extensions;
using Lynkly.Shared.Kernel.Core.Web;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseSwaggerSupport();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok("Lynkly Resolver API"));

app.Run();
