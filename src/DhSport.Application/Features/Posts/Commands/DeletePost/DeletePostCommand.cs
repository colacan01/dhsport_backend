using MediatR;

namespace DhSport.Application.Features.Posts.Commands.DeletePost;

public record DeletePostCommand(Guid Id, Guid UserId) : IRequest<bool>;
