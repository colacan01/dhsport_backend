using AutoMapper;
using DhSport.Application.DTOs.Board;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Content;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DhSport.Application.Features.Boards.Queries.GetBoard;

public class GetBoardQueryHandler : IRequestHandler<GetBoardQuery, BoardDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetBoardQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BoardDto?> Handle(GetBoardQuery request, CancellationToken cancellationToken)
    {
        var board = await _unitOfWork.Repository<Board>()
            .GetByIdAsync(request.Id, cancellationToken);

        if (board == null)
            return null;

        var boardDto = _mapper.Map<BoardDto>(board);

        // Get BoardType name
        var boardType = await _unitOfWork.Repository<BoardType>()
            .GetByIdAsync(board.BoardTypeId, cancellationToken);

        if (boardType != null)
            boardDto.BoardTypeNm = boardType.BoardTypeNm;

        // Get post count
        var postCount = await _unitOfWork.Repository<Post>()
            .GetQueryable()
            .Where(p => p.BoardId == board.Id && !p.IsDeleted)
            .CountAsync(cancellationToken);

        boardDto.PostCount = postCount;

        return boardDto;
    }
}
