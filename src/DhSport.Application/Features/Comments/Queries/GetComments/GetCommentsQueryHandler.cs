using AutoMapper;
using DhSport.Application.DTOs.Post;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Content;
using DhSport.Domain.Entities.UserManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DhSport.Application.Features.Comments.Queries.GetComments;

public class GetCommentsQueryHandler : IRequestHandler<GetCommentsQuery, List<CommentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCommentsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<CommentDto>> Handle(GetCommentsQuery request, CancellationToken cancellationToken)
    {
        // Get top-level comments only (ParentCommentId == null)
        // Filter out deleted comments
        var topLevelComments = await _unitOfWork.Repository<Comment>()
            .GetQueryable()
            .Where(c => c.PostId == request.PostId && !c.IsDeleted && c.ParentCommentId == null)
            .OrderBy(c => c.CreateDttm)
            .ToListAsync(cancellationToken);

        // Build hierarchical structure with author names
        return await MapCommentsAsync(topLevelComments, cancellationToken);
    }

    private async Task<List<CommentDto>> MapCommentsAsync(List<Comment> comments, CancellationToken cancellationToken)
    {
        var commentDtos = new List<CommentDto>();

        foreach (var comment in comments)
        {
            var commentDto = _mapper.Map<CommentDto>(comment);

            // Get author name
            var author = await _unitOfWork.Repository<User>().GetByIdAsync(comment.AuthorId, cancellationToken);
            if (author != null)
                commentDto.AuthorNm = author.UserNm;

            // Get replies recursively (filter out deleted replies)
            var replies = await _unitOfWork.Repository<Comment>()
                .GetQueryable()
                .Where(c => c.ParentCommentId == comment.Id && !c.IsDeleted)
                .OrderBy(c => c.CreateDttm)
                .ToListAsync(cancellationToken);

            if (replies.Any())
                commentDto.Replies = await MapCommentsAsync(replies, cancellationToken);

            commentDtos.Add(commentDto);
        }

        return commentDtos;
    }
}
