using DhSport.Application.DTOs.Board;
using MediatR;

namespace DhSport.Application.Features.Boards.Commands.UpdateBoard;

public record UpdateBoardCommand(Guid Id, UpdateBoardDto Dto) : IRequest<BoardDto>;
