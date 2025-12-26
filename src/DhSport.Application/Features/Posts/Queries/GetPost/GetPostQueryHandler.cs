using AutoMapper;
using DhSport.Application.DTOs.Post;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Content;
using DhSport.Domain.Entities.Features;
using DhSport.Domain.Entities.UserManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DhSport.Application.Features.Posts.Queries.GetPost;

public class GetPostQueryHandler : IRequestHandler<GetPostQuery, PostDetailDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPostQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PostDetailDto?> Handle(GetPostQuery request, CancellationToken cancellationToken)
    {
        var post = await _unitOfWork.Repository<Post>().GetByIdAsync(request.Id, cancellationToken);

        if (post == null || post.IsDeleted)
            return null;

        // Increase view count and log read
        if (request.IncreaseViewCount)
        {
            post.ViewCnt++;
            await _unitOfWork.Repository<Post>().UpdateAsync(post, cancellationToken);

            // Create read log
            var readLog = new ReadLog
            {
                PostId = post.Id,
                ReadUserId = request.UserId,
                IpAddress = request.IpAddress,
                CreateDttm = DateTime.UtcNow
            };
            await _unitOfWork.Repository<ReadLog>().AddAsync(readLog, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var postDto = _mapper.Map<PostDetailDto>(post);

        // Get related data
        var board = await _unitOfWork.Repository<Board>().GetByIdAsync(post.BoardId, cancellationToken);
        var postType = await _unitOfWork.Repository<PostType>().GetByIdAsync(post.PostTypeId, cancellationToken);
        var author = await _unitOfWork.Repository<User>().GetByIdAsync(post.AuthorId, cancellationToken);

        if (board != null) postDto.BoardNm = board.BoardNm;
        if (postType != null) postDto.PostTypeNm = postType.PostTypeNm;
        if (author != null) postDto.AuthorNm = author.UserNm;

        // Get files
        var files = await _unitOfWork.Repository<PostFile>()
            .GetQueryable()
            .Where(f => f.PostId == post.Id)
            .OrderBy(f => f.DisplayOrder)
            .ToListAsync(cancellationToken);

        postDto.Files = _mapper.Map<List<PostFileDto>>(files);

        // Get comments (top-level only, replies will be nested)
        var comments = await _unitOfWork.Repository<Comment>()
            .GetQueryable()
            .Where(c => c.PostId == post.Id && !c.IsDeleted && c.ParentCommentId == null)
            .OrderBy(c => c.CreateDttm)
            .ToListAsync(cancellationToken);

        postDto.Comments = await MapCommentsAsync(comments, cancellationToken);

        return postDto;
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
