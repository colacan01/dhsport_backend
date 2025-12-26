using DhSport.Application.DTOs.User;
using MediatR;

namespace DhSport.Application.Features.Users.Queries.GetUsers;

public record GetUsersQuery(int PageNumber = 1, int PageSize = 10) : IRequest<List<UserDto>>;
