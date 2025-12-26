using AutoMapper;
using DhSport.Application.DTOs.Post;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Content;
using DhSport.Domain.Entities.UserManagement;
using MediatR;

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
        var post = await _unitOfWork.Repository<Post>().GetByIdAsync(request.Id, cancellationToken);

        if (post == null)
            throw new KeyNotFoundException($"Post with ID {request.Id} not found");

        if (post.AuthorId != request.UserId)
            throw new UnauthorizedAccessException("You can only update your own posts");

        if (!string.IsNullOrEmpty(request.Dto.Title))
            post.Title = request.Dto.Title;

        if (request.Dto.PostContent != null)
            post.PostContent = request.Dto.PostContent;

        if (request.Dto.PostTypeId.HasValue)
            post.PostTypeId = request.Dto.PostTypeId.Value;

        if (request.Dto.IsNotice.HasValue)
            post.IsNotice = request.Dto.IsNotice.Value;

        if (request.Dto.IsSecret.HasValue)
            post.IsSecret = request.Dto.IsSecret.Value;

        post.UpdateDttm = DateTime.UtcNow;
        post.UpdateUserId = request.UserId;

        await _unitOfWork.Repository<Post>().UpdateAsync(post, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
}
