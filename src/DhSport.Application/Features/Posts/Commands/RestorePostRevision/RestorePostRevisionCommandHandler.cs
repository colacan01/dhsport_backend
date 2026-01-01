using AutoMapper;
using System.Text.Json;
using DhSport.Application.Common.Helpers;
using DhSport.Application.DTOs.Post;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Content;
using DhSport.Domain.Entities.Site;
using DhSport.Domain.Entities.UserManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DhSport.Application.Features.Posts.Commands.RestorePostRevision;

public class RestorePostRevisionCommandHandler : IRequestHandler<RestorePostRevisionCommand, PostDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RestorePostRevisionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PostDto> Handle(RestorePostRevisionCommand request, CancellationToken cancellationToken)
    {
        // Start transaction for atomic operations
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Get the revision to restore
            var revision = await _unitOfWork.Repository<PostRevision>()
                .GetQueryable()
                .Include(r => r.Post)
                .FirstOrDefaultAsync(r => r.Id == request.RevisionId && r.PostId == request.PostId, cancellationToken);

            if (revision == null)
                throw new KeyNotFoundException($"Revision with ID {request.RevisionId} not found");

            var post = revision.Post;

            if (post == null || post.IsDeleted)
                throw new KeyNotFoundException($"Post with ID {request.PostId} not found");

            // Check permissions: author or admin can restore
            if (!request.IsAdmin && post.AuthorId != request.UserId)
                throw new UnauthorizedAccessException("You can only restore your own post revisions");

            // Store current content as a new revision before restoring
            var currentContentJson = post.PostContent != null 
                ? JsonSerializer.Serialize(post.PostContent) 
                : string.Empty;

            var currentContentRevision = new PostRevision
            {
                PostId = post.Id,
                PostContent = currentContentJson,
                RevisionNote = $"복원 전 백업 (Revision ID: {request.RevisionId})",
                RevisionUserId = request.UserId,
                CreateDttm = DateTime.UtcNow
            };

            await _unitOfWork.Repository<PostRevision>().AddAsync(currentContentRevision, cancellationToken);

            // Parse the revision content from JSON string to object
            object? revisionContentObject = null;
            if (!string.IsNullOrEmpty(revision.PostContent))
            {
                revisionContentObject = JsonSerializer.Deserialize<object>(revision.PostContent);
            }

            // Check if content actually changed
            if (!JsonComparer.AreEqual(post.PostContent, revisionContentObject))
            {
                // Restore the content from the revision
                post.PostContent = revisionContentObject;
                post.UpdateDttm = DateTime.UtcNow;
                post.UpdateUserId = request.UserId;

                await _unitOfWork.Repository<Post>().UpdateAsync(post, cancellationToken);
            }

            // Send notification if admin restores another user's post
            if (request.IsAdmin && post.AuthorId != request.UserId)
            {
                var admin = await _unitOfWork.Repository<User>().GetByIdAsync(request.UserId, cancellationToken);
                var adminName = admin?.UserNm ?? "관리자";

                var notification = new Notification
                {
                    UserId = post.AuthorId,
                    NotificationType = "POST_RESTORED",
                    NotificationTitle = "게시물이 복원되었습니다",
                    NotificationContent = $"{adminName}님이 회원님의 게시물 '{post.Title}'을 이전 버전으로 복원했습니다.",
                    IsRead = false,
                    CreateDttm = DateTime.UtcNow
                };

                await _unitOfWork.Repository<Notification>().AddAsync(notification, cancellationToken);
            }

            // Maintain only last 10 revisions
            var revisionCount = await _unitOfWork.Repository<PostRevision>()
                .GetQueryable()
                .CountAsync(r => r.PostId == post.Id, cancellationToken);

            if (revisionCount > 10)
            {
                var revisionsToDelete = await _unitOfWork.Repository<PostRevision>()
                    .GetQueryable()
                    .Where(r => r.PostId == post.Id)
                    .OrderBy(r => r.CreateDttm)
                    .Take(revisionCount - 10)
                    .ToListAsync(cancellationToken);

                foreach (var oldRevision in revisionsToDelete)
                {
                    await _unitOfWork.Repository<PostRevision>().DeleteAsync(oldRevision, cancellationToken);
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
