using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Content;
using MediatR;

namespace DhSport.Application.Features.Boards.Commands.DeleteBoard;

public class DeleteBoardCommandHandler : IRequestHandler<DeleteBoardCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBoardCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteBoardCommand request, CancellationToken cancellationToken)
    {
        var board = await _unitOfWork.Repository<Board>().GetByIdAsync(request.Id, cancellationToken);

        if (board == null)
            return false;

        board.IsActive = false;
        board.UpdateDttm = DateTime.UtcNow;

        await _unitOfWork.Repository<Board>().UpdateAsync(board, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
