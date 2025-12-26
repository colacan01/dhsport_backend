using MediatR;

namespace DhSport.Application.Common.Interfaces;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
