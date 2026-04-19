using Microsoft.AspNetCore.Routing;

namespace Lynkly.Shared.Kernel.Core.Web;

public interface IEndpoint
{
    void MapEndpoints(IEndpointRouteBuilder app);
}
