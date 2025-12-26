using AutoMapper;
using DhSport.Application.DTOs.Board;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Content;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DhSport.Application.Features.Boards.Queries.GetBoards;

public class GetBoardsQueryHandler : IRequestHandler<GetBoardsQuery, List<BoardDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetBoardsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<BoardDto>> Handle(GetBoardsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Board>().GetQueryable();

        if (!request.IncludeInactive)
            query = query.Where(b => b.IsActive);

        if (request.BoardTypeId.HasValue)
            query = query.Where(b => b.BoardTypeId == request.BoardTypeId.Value);

        var boards = await query
            .OrderBy(b => b.DisplayOrder)
            .ThenBy(b => b.BoardNm)
            .ToListAsync(cancellationToken);

        var boardDtos = _mapper.Map<List<BoardDto>>(boards);

        // Get BoardType names
        var boardTypeIds = boards.Select(b => b.BoardTypeId).Distinct().ToList();
        var boardTypes = await _unitOfWork.Repository<BoardType>()
            .GetQueryable()
            .Where(bt => boardTypeIds.Contains(bt.Id))
            .ToListAsync(cancellationToken);

        var boardTypeDict = boardTypes.ToDictionary(bt => bt.Id, bt => bt.BoardTypeNm);

        // Get post counts
        var boardIds = boards.Select(b => b.Id).ToList();
        var postCounts = await _unitOfWork.Repository<Post>()
            .GetQueryable()
            .Where(p => boardIds.Contains(p.BoardId) && !p.IsDeleted)
            .GroupBy(p => p.BoardId)
            .Select(g => new { BoardId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var postCountDict = postCounts.ToDictionary(pc => pc.BoardId, pc => pc.Count);

        foreach (var boardDto in boardDtos)
        {
            if (boardTypeDict.TryGetValue(boardDto.BoardTypeId, out var typeName))
                boardDto.BoardTypeNm = typeName;

            if (postCountDict.TryGetValue(boardDto.Id, out var count))
                boardDto.PostCount = count;
        }

        return boardDtos;
    }
}
