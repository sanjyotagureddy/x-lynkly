using Lynkly.Resolver.API.Extensions;
using lynkly.ServiceDefaults;

using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();
builder.Services
	.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer();
builder.Services.AddAuthorization();
builder.Services.AddRequestContextSupport();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseRequestContextSupport();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok("Lynkly Resolver API"));

app.Run();
