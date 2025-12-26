using DhSport.Application.DTOs.Comment;
using DhSport.Application.DTOs.Post;
using MediatR;

namespace DhSport.Application.Features.Comments.Commands.UpdateComment;

public record UpdateCommentCommand(Guid Id, UpdateCommentDto Dto, Guid UserId) : IRequest<CommentDto>;
