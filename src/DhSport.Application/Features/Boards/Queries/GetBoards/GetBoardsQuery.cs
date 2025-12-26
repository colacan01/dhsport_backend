using DhSport.Application.DTOs.Board;
using MediatR;

namespace DhSport.Application.Features.Boards.Queries.GetBoards;

public record GetBoardsQuery(Guid? BoardTypeId = null, bool IncludeInactive = false) : IRequest<List<BoardDto>>;
