using DhSport.Application.DTOs.Board;
using MediatR;

namespace DhSport.Application.Features.Boards.Commands.CreateBoard;

public record CreateBoardCommand(CreateBoardDto Dto) : IRequest<BoardDto>;
