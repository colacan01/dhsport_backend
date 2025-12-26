using DhSport.Application.DTOs.User;
using MediatR;

namespace DhSport.Application.Features.Users.Commands.UpdateUser;

public record UpdateUserCommand(Guid Id, UpdateUserDto Dto) : IRequest<UserDto>;
