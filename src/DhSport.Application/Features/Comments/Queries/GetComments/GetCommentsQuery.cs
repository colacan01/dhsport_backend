using DhSport.Application.DTOs.Post;
using MediatR;

namespace DhSport.Application.Features.Comments.Queries.GetComments;

public record GetCommentsQuery(Guid PostId) : IRequest<List<CommentDto>>;
