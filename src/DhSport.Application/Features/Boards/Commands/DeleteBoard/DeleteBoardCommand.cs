using MediatR;

namespace DhSport.Application.Features.Boards.Commands.DeleteBoard;

public record DeleteBoardCommand(Guid Id) : IRequest<bool>;
