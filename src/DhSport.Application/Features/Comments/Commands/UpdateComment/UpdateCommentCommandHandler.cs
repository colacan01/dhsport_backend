using AutoMapper;
using DhSport.Application.DTOs.Post;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Content;
using DhSport.Domain.Entities.UserManagement;
using MediatR;

namespace DhSport.Application.Features.Comments.Commands.UpdateComment;

public class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, CommentDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateCommentCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CommentDto> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _unitOfWork.Repository<Comment>().GetByIdAsync(request.Id, cancellationToken);

        if (comment == null || comment.IsDeleted)
            throw new KeyNotFoundException($"Comment with ID {request.Id} not found");

        if (comment.AuthorId != request.UserId)
            throw new UnauthorizedAccessException("You can only update your own comments");

        if (request.Dto.CommentContent != null)
            comment.CommentContent = request.Dto.CommentContent;

        comment.UpdateDttm = DateTime.UtcNow;

        await _unitOfWork.Repository<Comment>().UpdateAsync(comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var commentDto = _mapper.Map<CommentDto>(comment);

        // Get author name
        var author = await _unitOfWork.Repository<User>().GetByIdAsync(comment.AuthorId, cancellationToken);
        if (author != null)
            commentDto.AuthorNm = author.UserNm;

        return commentDto;
    }
}
