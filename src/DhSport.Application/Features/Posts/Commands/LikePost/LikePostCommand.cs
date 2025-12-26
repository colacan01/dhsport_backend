using MediatR;

namespace DhSport.Application.Features.Posts.Commands.LikePost;

public record LikePostCommand(Guid PostId, Guid UserId) : IRequest<bool>;
