using DhSport.Application.DTOs.Post;
using MediatR;

namespace DhSport.Application.Features.Posts.Queries.GetPostRevisionDiff;

public record GetPostRevisionDiffQuery(
    Guid FromRevisionId,
    Guid ToRevisionId,
    Guid? RequestUserId = null,
    bool IsAdmin = false) : IRequest<PostRevisionDiffDto?>;
