using DhSport.Application.DTOs.Post;
using MediatR;

namespace DhSport.Application.Features.Posts.Commands.CreatePost;

public record CreatePostCommand(CreatePostDto Dto, Guid AuthorId) : IRequest<PostDto>;
