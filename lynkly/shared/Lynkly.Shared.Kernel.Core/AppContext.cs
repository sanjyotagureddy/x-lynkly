using Lynkly.Shared.Kernel.Core.Context;

namespace Lynkly.Shared.Kernel.Core;

/// <inheritdoc />
public class AppContext(string serviceName) : RequestContext(serviceName)
{
    public static new AppContext Current
    {
        get
        {
            var current = RequestContextScope.Current;
            return current as AppContext ?? FromRequestContext(current);
        }
    }

    public static new AppContext FromHttpContext(HttpContext httpContext, string serviceName)
    {
        var requestContext = RequestContext.FromHttpContext(httpContext, serviceName);
        return FromRequestContext(requestContext);
    }

    private static AppContext FromRequestContext(RequestContext context)
    {
        return new AppContext(context.ServiceName)
        {
            CorrelationId = context.CorrelationId,
            TransactionId = context.TransactionId,
            RequestId = context.RequestId,
            TraceId = context.TraceId,
            TenantId = context.TenantId,
            SessionId = context.SessionId,
            RequestStartTimestampUtc = context.RequestStartTimestampUtc,
            Headers = context.Headers
        };
    }
}