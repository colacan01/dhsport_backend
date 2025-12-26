using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Content;
using MediatR;

namespace DhSport.Application.Features.Comments.Commands.DeleteComment;

public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCommentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _unitOfWork.Repository<Comment>().GetByIdAsync(request.Id, cancellationToken);

        if (comment == null || comment.IsDeleted)
            return false;

        if (comment.AuthorId != request.UserId)
            throw new UnauthorizedAccessException("You can only delete your own comments");

        comment.IsDeleted = true;
        comment.UpdateDttm = DateTime.UtcNow;

        await _unitOfWork.Repository<Comment>().UpdateAsync(comment, cancellationToken);

        // Update post comment count
        var post = await _unitOfWork.Repository<Post>().GetByIdAsync(comment.PostId, cancellationToken);
        if (post != null && post.CommentCnt > 0)
        {
            post.CommentCnt--;
            await _unitOfWork.Repository<Post>().UpdateAsync(post, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
