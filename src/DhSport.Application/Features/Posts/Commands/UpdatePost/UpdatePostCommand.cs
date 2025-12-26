using DhSport.Application.DTOs.Post;
using MediatR;

namespace DhSport.Application.Features.Posts.Commands.UpdatePost;

public record UpdatePostCommand(Guid Id, UpdatePostDto Dto, Guid UserId) : IRequest<PostDto>;
