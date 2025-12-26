using DhSport.Application.DTOs.Board;
using MediatR;

namespace DhSport.Application.Features.Boards.Queries.GetBoard;

public record GetBoardQuery(Guid Id) : IRequest<BoardDto?>;
