using AutoMapper;
using DhSport.Application.DTOs.Post;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Content;
using DhSport.Domain.Entities.UserManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DhSport.Application.Features.Comments.Queries.GetComment;

public class GetCommentQueryHandler : IRequestHandler<GetCommentQuery, CommentDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCommentQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CommentDto?> Handle(GetCommentQuery request, CancellationToken cancellationToken)
    {
        var comment = await _unitOfWork.Repository<Comment>().GetByIdAsync(request.Id, cancellationToken);

        // Return null if comment not found or deleted
        if (comment == null || comment.IsDeleted)
            return null;

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

        return commentDto;
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

            // Get replies recursively
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
