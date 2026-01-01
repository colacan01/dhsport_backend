using AutoMapper;
using DhSport.Application.DTOs.Post;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Content;
using DhSport.Domain.Entities.UserManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DhSport.Application.Features.Posts.Queries.GetPostRevisions;

public class GetPostRevisionsQueryHandler : IRequestHandler<GetPostRevisionsQuery, List<PostRevisionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPostRevisionsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<PostRevisionDto>> Handle(GetPostRevisionsQuery request, CancellationToken cancellationToken)
    {
        // Get the post to verify permissions
        var post = await _unitOfWork.Repository<Post>().GetByIdAsync(request.PostId, cancellationToken);

        if (post == null || post.IsDeleted)
            return new List<PostRevisionDto>();

        // Check if user has permission to view revisions (author or admin)
        if (!request.IsAdmin && request.RequestUserId != post.AuthorId)
            return new List<PostRevisionDto>();

        // Get revisions with related data
        var revisions = await _unitOfWork.Repository<PostRevision>()
            .GetQueryable()
            .Include(r => r.Post)
            .ThenInclude(p => p.Author)
            .Where(r => r.PostId == request.PostId)
            .OrderByDescending(r => r.CreateDttm)
            .ToListAsync(cancellationToken);

        var revisionDtos = _mapper.Map<List<PostRevisionDto>>(revisions);

        // Map revision user names
        foreach (var dto in revisionDtos)
        {
            var revision = revisions.First(r => r.Id == dto.Id);
            if (revision.Post?.Author != null)
            {
                dto.RevisionUserNm = revision.Post.Author.UserNm;
            }
            else if (revision.RevisionUserId != Guid.Empty)
            {
                var user = await _unitOfWork.Repository<User>().GetByIdAsync(revision.RevisionUserId, cancellationToken);
                if (user != null)
                    dto.RevisionUserNm = user.UserNm;
            }
        }

        return revisionDtos;
    }
}
