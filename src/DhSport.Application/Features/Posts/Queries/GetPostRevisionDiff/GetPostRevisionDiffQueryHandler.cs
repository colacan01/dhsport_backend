using DhSport.Application.Common.Helpers;
using DhSport.Application.DTOs.Post;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Content;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DhSport.Application.Features.Posts.Queries.GetPostRevisionDiff;

public class GetPostRevisionDiffQueryHandler : IRequestHandler<GetPostRevisionDiffQuery, PostRevisionDiffDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPostRevisionDiffQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PostRevisionDiffDto?> Handle(GetPostRevisionDiffQuery request, CancellationToken cancellationToken)
    {
        // Get both revisions
        var fromRevision = await _unitOfWork.Repository<PostRevision>()
            .GetQueryable()
            .Include(r => r.Post)
            .FirstOrDefaultAsync(r => r.Id == request.FromRevisionId, cancellationToken);

        var toRevision = await _unitOfWork.Repository<PostRevision>()
            .GetQueryable()
            .Include(r => r.Post)
            .FirstOrDefaultAsync(r => r.Id == request.ToRevisionId, cancellationToken);

        if (fromRevision == null || toRevision == null)
            return null;

        // Revisions must be from the same post
        if (fromRevision.PostId != toRevision.PostId)
            return null;

        // Check if user has permission to view revisions (author or admin)
        if (!request.IsAdmin && request.RequestUserId != fromRevision.Post.AuthorId)
            return null;

        // Compare revisions using JsonComparer
        var diff = JsonComparer.GetDifferences(
            request.FromRevisionId,
            request.ToRevisionId,
            fromRevision.PostContent,
            toRevision.PostContent);

        return diff;
    }
}
