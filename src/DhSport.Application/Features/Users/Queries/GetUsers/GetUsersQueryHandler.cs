using AutoMapper;
using DhSport.Application.DTOs.User;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.UserManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DhSport.Application.Features.Users.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetUsersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Repository<User>()
            .GetQueryable()
            .Where(u => !u.IsDeleted)
            .OrderByDescending(u => u.CreateDttm)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var userDtos = _mapper.Map<List<UserDto>>(users);

        // Get roles for all users
        var userIds = users.Select(u => u.Id).ToList();
        var userRoles = await _unitOfWork.Repository<UserRoleMap>()
            .GetQueryable()
            .Where(ur => userIds.Contains(ur.UserId) && ur.IsActive)
            .Include(ur => ur.Role)
            .ToListAsync(cancellationToken);

        foreach (var userDto in userDtos)
        {
            userDto.Roles = userRoles
                .Where(ur => ur.UserId == userDto.Id)
                .Select(ur => ur.Role.RoleNm)
                .ToList();
        }

        return userDtos;
    }
}
