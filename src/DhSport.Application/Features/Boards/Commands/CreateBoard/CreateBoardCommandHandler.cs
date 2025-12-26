using AutoMapper;
using DhSport.Application.DTOs.Board;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Content;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DhSport.Application.Features.Boards.Commands.CreateBoard;

public class CreateBoardCommandHandler : IRequestHandler<CreateBoardCommand, BoardDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateBoardCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BoardDto> Handle(CreateBoardCommand request, CancellationToken cancellationToken)
    {
        var board = new Board
        {
            BoardNm = request.Dto.BoardNm,
            BoardDesc = request.Dto.BoardDesc,
            BoardTypeId = request.Dto.BoardTypeId,
            DisplayOrder = request.Dto.DisplayOrder,
            IsActive = true,
            CreateDttm = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Board>().AddAsync(board, cancellationToken);
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
