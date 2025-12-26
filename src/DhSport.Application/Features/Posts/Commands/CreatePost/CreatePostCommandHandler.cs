using AutoMapper;
using DhSport.Application.DTOs.Post;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Content;
using DhSport.Domain.Entities.UserManagement;
using MediatR;

namespace DhSport.Application.Features.Posts.Commands.CreatePost;

public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, PostDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreatePostCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PostDto> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var post = new Post
        {
            BoardId = request.Dto.BoardId,
            PostTypeId = request.Dto.PostTypeId,
            Title = request.Dto.Title,
            PostContent = request.Dto.PostContent,
            AuthorId = request.AuthorId,
            IsNotice = request.Dto.IsNotice,
            IsSecret = request.Dto.IsSecret,
            ViewCnt = 0,
            LikeCnt = 0,
            CommentCnt = 0,
            CreateDttm = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Post>().AddAsync(post, cancellationToken);
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
