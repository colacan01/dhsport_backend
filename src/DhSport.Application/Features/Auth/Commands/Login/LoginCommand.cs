using DhSport.Application.DTOs.User;
using MediatR;

namespace DhSport.Application.Features.Auth.Commands.Login;

public record LoginCommand(LoginDto Dto) : IRequest<LoginResponseDto>;
