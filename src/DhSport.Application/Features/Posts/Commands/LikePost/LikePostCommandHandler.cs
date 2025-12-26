using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Content;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DhSport.Application.Features.Posts.Commands.LikePost;

public class LikePostCommandHandler : IRequestHandler<LikePostCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public LikePostCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(LikePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _unitOfWork.Repository<Post>().GetByIdAsync(request.PostId, cancellationToken);
        if (post == null)
            throw new KeyNotFoundException($"Post with ID {request.PostId} not found");

        // Check if user already liked this post
        var existingLike = await _unitOfWork.Repository<PostLike>()
            .GetQueryable()
            .FirstOrDefaultAsync(pl => pl.PostId == request.PostId && pl.UserId == request.UserId, cancellationToken);

        if (existingLike != null)
        {
            // Unlike - remove the like
            await _unitOfWork.Repository<PostLike>().DeleteAsync(existingLike, cancellationToken);

            // Decrease like count
            post.LikeCnt = Math.Max(0, post.LikeCnt - 1);
            await _unitOfWork.Repository<Post>().UpdateAsync(post, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return false; // Unliked
        }
        else
        {
            // Like - add new like
            var postLike = new PostLike
            {
                PostId = request.PostId,
                UserId = request.UserId,
                CreateDttm = DateTime.UtcNow
            };

            await _unitOfWork.Repository<PostLike>().AddAsync(postLike, cancellationToken);

            // Increase like count
            post.LikeCnt++;
            await _unitOfWork.Repository<Post>().UpdateAsync(post, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true; // Liked
        }
    }
}
