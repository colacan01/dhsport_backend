using DhSport.Application.DTOs.Post;
using MediatR;

namespace DhSport.Application.Features.Posts.Queries.GetPost;

public record GetPostQuery(
    Guid Id,
    bool IncreaseViewCount = true,
    Guid? UserId = null,
    string? IpAddress = null) : IRequest<PostDetailDto?>;
