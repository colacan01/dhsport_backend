using AutoMapper;
using System.Text.Json;
using DhSport.Application.Common.Helpers;
using DhSport.Application.DTOs.Post;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Content;
using DhSport.Domain.Entities.UserManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DhSport.Application.Features.Posts.Commands.UpdatePost;

public class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommand, PostDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdatePostCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PostDto> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        // Start transaction for atomic operations
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var post = await _unitOfWork.Repository<Post>().GetByIdAsync(request.Id, cancellationToken);

            if (post == null)
                throw new KeyNotFoundException($"Post with ID {request.Id} not found");

            if (post.AuthorId != request.UserId)
                throw new UnauthorizedAccessException("You can only update your own posts");

            // Store original content for revision tracking
            var originalContent = post.PostContent;
            var contentChanged = false;

            if (!string.IsNullOrEmpty(request.Dto.Title))
                post.Title = request.Dto.Title;

            if (request.Dto.PostContent != null)
            {
                // Check if content actually changed using JsonComparer
                if (!JsonComparer.AreEqual(originalContent, request.Dto.PostContent))
                {
                    contentChanged = true;
                    post.PostContent = request.Dto.PostContent;
                }
            }

            if (request.Dto.PostTypeId.HasValue)
                post.PostTypeId = request.Dto.PostTypeId.Value;

            if (request.Dto.IsNotice.HasValue)
                post.IsNotice = request.Dto.IsNotice.Value;

            if (request.Dto.IsSecret.HasValue)
                post.IsSecret = request.Dto.IsSecret.Value;

            post.UpdateDttm = DateTime.UtcNow;
            post.UpdateUserId = request.UserId;

            await _unitOfWork.Repository<Post>().UpdateAsync(post, cancellationToken);

            // Create revision if content changed
            if (contentChanged)
            {
                // Serialize original content to JSON string for storage in PostRevision
                var originalContentJson = originalContent != null 
                    ? JsonSerializer.Serialize(originalContent) 
                    : string.Empty;

                var revision = new PostRevision
                {
                    PostId = post.Id,
                    PostContent = originalContentJson,
                    RevisionNote = string.IsNullOrEmpty(request.Dto.RevisionNote) ? "자동 저장" : request.Dto.RevisionNote,
                    RevisionUserId = request.UserId,
                    CreateDttm = DateTime.UtcNow
                };

                await _unitOfWork.Repository<PostRevision>().AddAsync(revision, cancellationToken);

                // Maintain only last 10 revisions
                var revisionCount = await _unitOfWork.Repository<PostRevision>()
                    .GetQueryable()
                    .CountAsync(r => r.PostId == post.Id, cancellationToken);

                if (revisionCount >= 10)
                {
                    // Delete oldest revisions to keep only 9 (so after adding new one, total is 10)
                    var revisionsToDelete = await _unitOfWork.Repository<PostRevision>()
                        .GetQueryable()
                        .Where(r => r.PostId == post.Id)
                        .OrderBy(r => r.CreateDttm)
                        .Take(revisionCount - 9)
                        .ToListAsync(cancellationToken);

                    foreach (var oldRevision in revisionsToDelete)
                    {
                        await _unitOfWork.Repository<PostRevision>().DeleteAsync(oldRevision, cancellationToken);
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var postDto = _mapper.Map<PostDto>(post);

            // Get related data
            var board = await _unitOfWork.Repository<Board>().GetByIdAsync(post.BoardId, cancellationToken);
            var postType = await _unitOfWork.Repository<PostType>().GetByIdAsync(post.PostTypeId, cancellationToken);
            var author = await _unitOfWork.Repository<User>().GetByIdAsync(post.AuthorId, cancellationToken);

            if (board != null) postDto.BoardNm = board.BoardNm;
            if (postType != null) postDto.PostTypeNm = postType.PostTypeNm;
            if (author != null) postDto.AuthorNm = author.UserNm;

            return postDto;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
