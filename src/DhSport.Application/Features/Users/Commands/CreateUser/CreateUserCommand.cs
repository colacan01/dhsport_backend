using DhSport.Application.DTOs.User;
using MediatR;

namespace DhSport.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand(CreateUserDto Dto) : IRequest<UserDto>;
