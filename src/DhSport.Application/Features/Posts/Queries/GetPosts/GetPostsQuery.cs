using DhSport.Application.DTOs.Post;
using MediatR;

namespace DhSport.Application.Features.Posts.Queries.GetPosts;

public record GetPostsQuery(
    Guid? BoardId = null,
    Guid? PostTypeId = null,
    string? SearchKeyword = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<List<PostDto>>;
