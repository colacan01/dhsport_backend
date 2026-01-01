using DhSport.Application.DTOs.Post;
using MediatR;

namespace DhSport.Application.Features.Posts.Commands.RestorePostRevision;

public record RestorePostRevisionCommand(
    Guid PostId,
    Guid RevisionId,
    Guid UserId,
    bool IsAdmin = false) : IRequest<PostDto>;
