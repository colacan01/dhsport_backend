using DhSport.Application.DTOs.Auth;
using MediatR;

namespace DhSport.Application.Features.Auth.Commands.Register;

public record RegisterCommand(RegisterDto Dto) : IRequest<RegisterResponseDto>;
