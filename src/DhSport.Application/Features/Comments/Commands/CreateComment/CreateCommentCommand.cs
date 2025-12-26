using DhSport.Application.DTOs.Comment;
using DhSport.Application.DTOs.Post;
using MediatR;

namespace DhSport.Application.Features.Comments.Commands.CreateComment;

public record CreateCommentCommand(CreateCommentDto Dto, Guid AuthorId) : IRequest<CommentDto>;
