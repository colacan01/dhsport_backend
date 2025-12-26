using AutoMapper;
using DhSport.Application.DTOs.Board;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Content;
using MediatR;

namespace DhSport.Application.Features.Boards.Commands.UpdateBoard;

public class UpdateBoardCommandHandler : IRequestHandler<UpdateBoardCommand, BoardDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateBoardCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BoardDto> Handle(UpdateBoardCommand request, CancellationToken cancellationToken)
    {
        var board = await _unitOfWork.Repository<Board>().GetByIdAsync(request.Id, cancellationToken);

        if (board == null)
            throw new KeyNotFoundException($"Board with ID {request.Id} not found");

        if (!string.IsNullOrEmpty(request.Dto.BoardNm))
            board.BoardNm = request.Dto.BoardNm;

        if (request.Dto.BoardDesc != null)
            board.BoardDesc = request.Dto.BoardDesc;

        if (request.Dto.BoardTypeId.HasValue)
            board.BoardTypeId = request.Dto.BoardTypeId.Value;

        if (request.Dto.DisplayOrder.HasValue)
            board.DisplayOrder = request.Dto.DisplayOrder.Value;

        if (request.Dto.IsActive.HasValue)
            board.IsActive = request.Dto.IsActive.Value;

        board.UpdateDttm = DateTime.UtcNow;

        await _unitOfWork.Repository<Board>().UpdateAsync(board, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var boardDto = _mapper.Map<BoardDto>(board);

        // Get BoardType name
        var boardType = await _unitOfWork.Repository<BoardType>()
            .GetByIdAsync(board.BoardTypeId, cancellationToken);

        if (boardType != null)
            boardDto.BoardTypeNm = boardType.BoardTypeNm;

        return boardDto;
    }
}
