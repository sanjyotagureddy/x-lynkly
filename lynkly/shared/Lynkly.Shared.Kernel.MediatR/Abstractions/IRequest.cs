namespace Lynkly.Shared.Kernel.MediatR.Abstractions;

public interface IBaseRequest;

public interface IRequest : IBaseRequest;

public interface IRequest<out TResponse> : IBaseRequest;
