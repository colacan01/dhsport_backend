using DhSport.Application.DTOs.Post;
using MediatR;

namespace DhSport.Application.Features.Posts.Queries.GetPostRevisionById;

public record GetPostRevisionByIdQuery(
    Guid Id,
    Guid? RequestUserId = null,
    bool IsAdmin = false) : IRequest<PostRevisionDto?>;
