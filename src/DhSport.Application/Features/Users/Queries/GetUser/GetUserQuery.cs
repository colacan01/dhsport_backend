using DhSport.Application.DTOs.User;
using MediatR;

namespace DhSport.Application.Features.Users.Queries.GetUser;

public record GetUserQuery(Guid Id) : IRequest<UserDto?>;
