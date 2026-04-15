using Lynkly.Shared.Kernel.Context;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace Lynkly.Shared.Kernel.Context
{
    internal interface IRequestContextEnricher
    {
        void EnrichRequest(HttpContext httpContext, AppContext appContext);

        void EnrichResponse(HttpContext httpContext, AppContext appContext);
    }

    internal sealed class AppContext
    {
        private AppContext(
            string applicationName,
            string traceIdentifier,
            string method,
            string path,
            IReadOnlyDictionary<string, object?> items)
        {
            ApplicationName = applicationName;
            TraceIdentifier = traceIdentifier;
            Method = method;
            Path = path;
            Items = items;
        }

        public string ApplicationName { get; }

        public string TraceIdentifier { get; }

        public string Method { get; }

        public string Path { get; }

        public IReadOnlyDictionary<string, object?> Items { get; }

        public static AppContext FromHttpContext(HttpContext httpContext, string applicationName)
        {
            ArgumentNullException.ThrowIfNull(httpContext);

            return new AppContext(
                applicationName,
                httpContext.TraceIdentifier,
                httpContext.Request.Method,
                httpContext.Request.Path.ToString(),
                new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>
                {
                    ["RequestAborted"] = httpContext.RequestAborted,
                    ["Scheme"] = httpContext.Request.Scheme,
                    ["Host"] = httpContext.Request.Host.ToString()
                }));
        }
    }

    internal static class RequestContextScope
    {
        private static readonly AsyncLocal<AppContext?> CurrentContext = new();

        public static AppContext? Current => CurrentContext.Value;

        public static IDisposable BeginScope(AppContext appContext)
        {
            ArgumentNullException.ThrowIfNull(appContext);

            var previous = CurrentContext.Value;
            CurrentContext.Value = appContext;
            return new Scope(previous);
        }

        private sealed class Scope(AppContext? previous) : IDisposable
        {
            private bool disposed;

            public void Dispose()
            {
                if (disposed)
                {
                    return;
                }

                CurrentContext.Value = previous;
                disposed = true;
            }
        }
    }
}
namespace Lynkly.Resolver.API.Middlewares;

internal sealed class RequestContextMiddleware(
    RequestDelegate next,
    IHostEnvironment environment,
    IEnumerable<IRequestContextEnricher> enrichers)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var appContext = AppContext.FromHttpContext(httpContext, environment.ApplicationName);

        foreach (var enricher in enrichers)
        {
            enricher.EnrichRequest(httpContext, appContext);
        }

        using (RequestContextScope.BeginScope(appContext))
        {
            httpContext.Response.OnStarting(() =>
            {
                foreach (var enricher in enrichers)
                {
                    enricher.EnrichResponse(httpContext, appContext);
                }

                return Task.CompletedTask;
            });

            await next(httpContext);
        }
    }
}
