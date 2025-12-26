using DhSport.Application.DTOs.Post;
using MediatR;

namespace DhSport.Application.Features.Comments.Queries.GetComment;

public record GetCommentQuery(Guid Id) : IRequest<CommentDto?>;
