using AutoMapper;
using DhSport.Application.DTOs.Post;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Content;
using DhSport.Domain.Entities.UserManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DhSport.Application.Features.Posts.Queries.GetPostRevisionById;

public class GetPostRevisionByIdQueryHandler : IRequestHandler<GetPostRevisionByIdQuery, PostRevisionDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPostRevisionByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PostRevisionDto?> Handle(GetPostRevisionByIdQuery request, CancellationToken cancellationToken)
    {
        // Get revision with related data
        var revision = await _unitOfWork.Repository<PostRevision>()
            .GetQueryable()
            .Include(r => r.Post)
            .ThenInclude(p => p.Author)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (revision == null)
            return null;

        // Check if user has permission to view revision (author or admin)
        if (!request.IsAdmin && request.RequestUserId != revision.Post.AuthorId)
            return null;

        var revisionDto = _mapper.Map<PostRevisionDto>(revision);

        // Map revision user name
        if (revision.Post?.Author != null)
        {
            revisionDto.RevisionUserNm = revision.Post.Author.UserNm;
        }
        else if (revision.RevisionUserId != Guid.Empty)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(revision.RevisionUserId, cancellationToken);
            if (user != null)
                revisionDto.RevisionUserNm = user.UserNm;
        }

        return revisionDto;
    }
}
