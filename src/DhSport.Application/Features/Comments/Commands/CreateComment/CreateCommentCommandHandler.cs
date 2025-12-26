using AutoMapper;
using DhSport.Application.DTOs.Post;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Content;
using DhSport.Domain.Entities.UserManagement;
using MediatR;

namespace DhSport.Application.Features.Comments.Commands.CreateComment;

public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, CommentDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCommentCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CommentDto> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = new Comment
        {
            PostId = request.Dto.PostId,
            ParentCommentId = request.Dto.ParentCommentId,
            AuthorId = request.AuthorId,
            CommentContent = request.Dto.CommentContent,
            LikeCnt = 0,
            CreateDttm = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Comment>().AddAsync(comment, cancellationToken);

        // Update post comment count
        var post = await _unitOfWork.Repository<Post>().GetByIdAsync(request.Dto.PostId, cancellationToken);
        if (post != null)
        {
            post.CommentCnt++;
            await _unitOfWork.Repository<Post>().UpdateAsync(post, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var commentDto = _mapper.Map<CommentDto>(comment);

        // Get author name
        var author = await _unitOfWork.Repository<User>().GetByIdAsync(comment.AuthorId, cancellationToken);
        if (author != null)
            commentDto.AuthorNm = author.UserNm;

        return commentDto;
    }
}
