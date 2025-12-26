using MediatR;

namespace DhSport.Application.Features.Comments.Commands.LikeComment;

public record LikeCommentCommand(Guid CommentId, Guid UserId) : IRequest<bool>;
