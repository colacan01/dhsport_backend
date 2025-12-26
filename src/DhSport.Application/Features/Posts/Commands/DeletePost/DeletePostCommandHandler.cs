using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Content;
using MediatR;

namespace DhSport.Application.Features.Posts.Commands.DeletePost;

public class DeletePostCommandHandler : IRequestHandler<DeletePostCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeletePostCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _unitOfWork.Repository<Post>().GetByIdAsync(request.Id, cancellationToken);

        if (post == null)
            return false;

        if (post.AuthorId != request.UserId)
            throw new UnauthorizedAccessException("You can only delete your own posts");

        post.IsDeleted = true;
        post.UpdateDttm = DateTime.UtcNow;
        post.UpdateUserId = request.UserId;

        await _unitOfWork.Repository<Post>().UpdateAsync(post, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
