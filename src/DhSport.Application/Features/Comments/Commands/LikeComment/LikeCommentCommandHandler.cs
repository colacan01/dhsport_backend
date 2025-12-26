using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Content;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DhSport.Application.Features.Comments.Commands.LikeComment;

public class LikeCommentCommandHandler : IRequestHandler<LikeCommentCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public LikeCommentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(LikeCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _unitOfWork.Repository<Comment>().GetByIdAsync(request.CommentId, cancellationToken);
        if (comment == null)
            throw new KeyNotFoundException($"Comment with ID {request.CommentId} not found");

        // Check if user already liked this comment
        var existingLike = await _unitOfWork.Repository<CommentLike>()
            .GetQueryable()
            .FirstOrDefaultAsync(cl => cl.CommentId == request.CommentId && cl.UserId == request.UserId, cancellationToken);

        if (existingLike != null)
        {
            // Unlike - remove the like
            await _unitOfWork.Repository<CommentLike>().DeleteAsync(existingLike, cancellationToken);

            // Decrease like count
            comment.LikeCnt = Math.Max(0, comment.LikeCnt - 1);
            await _unitOfWork.Repository<Comment>().UpdateAsync(comment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return false; // Unliked
        }
        else
        {
            // Like - add new like
            var commentLike = new CommentLike
            {
                CommentId = request.CommentId,
                UserId = request.UserId,
                CreateDttm = DateTime.UtcNow
            };

            await _unitOfWork.Repository<CommentLike>().AddAsync(commentLike, cancellationToken);

            // Increase like count
            comment.LikeCnt++;
            await _unitOfWork.Repository<Comment>().UpdateAsync(comment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true; // Liked
        }
    }
}
