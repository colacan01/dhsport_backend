using MediatR;

namespace DhSport.Application.Features.Auth.Commands.Logout;

public record LogoutCommand(Guid UserId) : IRequest<Unit>;
