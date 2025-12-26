using MediatR;

namespace DhSport.Application.Features.Comments.Commands.DeleteComment;

public record DeleteCommentCommand(Guid Id, Guid UserId) : IRequest<bool>;
