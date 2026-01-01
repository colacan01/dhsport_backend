using DhSport.Application.DTOs.Post;
using MediatR;

namespace DhSport.Application.Features.Posts.Queries.GetPostRevisions;

public record GetPostRevisionsQuery(
    Guid PostId,
    Guid? RequestUserId = null,
    bool IsAdmin = false) : IRequest<List<PostRevisionDto>>;
